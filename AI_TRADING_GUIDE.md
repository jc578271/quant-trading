# Quantum Trading System: cTrader to Python AI to MT5

Hệ thống giao dịch thuật toán (Algorithmic Trading) hoàn chỉnh, kết nối luồng dữ liệu thời gian thực và lịch sử từ phần mềm cTrader sang backend Python để phân tích bằng AI (Machine Learning), sau đó phát tín hiệu giao dịch tự động sang MetaTrader 5 (MT5).

## 1. Kiến trúc hệ thống
Hệ thống được chia làm 3 mảng chức năng chính:
1. **Dữ liệu mỏ neo (cTrader):** 3 Indicator Exporter chạy trên cTrader, đọc dữ liệu Footprint, Volume Profile, Weis Wave và bắn JSON qua TCP Socket.
2. **Trung tâm não bộ (Python AI):** `socket_server` nhận dữ liệu, `ai_analyzer` gộp (aggregate) các mảng dữ liệu rời rạc lại thành trạng thái thị trường (`symbol_state`), trích xuất Feature và đưa vào Model dự đoán.
3. **Thực thi lệnh (MT5):** Nhận tín hiệu BUY/SELL từ AI và tự động quản lý Risk, đặt lệnh, TP/SL qua `mt5_client.py`.

---

## 2. Cấu trúc Codebase

### A. Phía cTrader (`ctrader-indicators/Raw Source Code/`)
Các file biểu đồ mã nguồn C# nguyên bản đã được trang bị Socket Native. 
- **`Order Flow Aggregated v2.0.cs`**: Xuất toàn bộ mảng dữ liệu Footprint (Volume Levels, Bids, Asks, Delta, Max/Min Delta).
- **`Weis & Wyckoff System v2.0.cs`**: Xuất thông tin cấu trúc sóng (Wave Volume, Direction, Price, ZigZag).
- **`Free Volume Profile v2.0.cs`**: Xuất thông tin phân bổ giao dịch (POC, VAH, VAL, Total Volume).

*Tính năng Export History Data:* Trong menu setting của cả 3 indicator có biến `Export History Data = True/False`. Khi bật `True` và thả indicator vào chart, nó sẽ cào sạch dữ liệu lịch sử và bắn liên tục sang Python để AI học tập. 

### B. Phía Python (`main/`)
- **`main.py`**: File Entry point. Nó khởi tạo kết nối Terminal MT5, bật Socket Server (Mặc định Port 5555) và gắn AI Analyzer vào vòng lặp xử lý.
- **`socket_server.py`**: Điểm đón dữ liệu. Sử dụng `asyncio` để nhận hàng trăm ngàn dòng JSON realtime. Cơ chế đọc từng dòng (`readline()`) chống rách gói tin, đặc biệt là gói Footprint khổng lồ.
- **`ai_analyzer.py`**: Bộ não. Hệ thống gom các mảnh JSON từ 3 indicator vào chung 1 biến dict `self.symbol_state[symbol]`. Khi thấy có đủ Data (Volume, Wave, VP), nó sẽ đưa cho `model.pkl` (Random Forest Classifier giả lập) ra quyết định MUA/BÁN theo xác suất > 65%. 
- **`mt5_client.py`**: Cầu nối đẩy lệnh mua bán vào MetaTrader 5 thật với Parameter Risk Quản lý vốn, tự tính toán điểm dừng lỗ Dựa trên tổng Balance hiện tại.

---

## 3. Hướng dẫn sử dụng & Chạy lệnh

### Bước 1: Chuẩn bị Terminal MT5
1. Mở MetaTrader 5, đăng nhập tài khoản.
2. Bật cờ **AutoTrading** (Nút Algo Trading hiện nút Play xanh).
3. Bật API kết nối trong: *Tools -> Options -> Expert Advisors -> Tích vào "Allow algorithmic trading"*.

### Bước 2: Bật Cụm Mắt Nhìn AI trên Python
Mở terminal, trỏ vào thư mục `main/` và gõ:
```bash
pip install -r requirements.txt
python main.py
```
*(Nếu là lần đầu chạy, code sẽ hiện log `No AI model found` và tự động sinh ra một Dummy Model `model.pkl` khù khờ để giả lập, sau đó server sẽ hiện báo Listen ở cổng 5555).*

### Bước 3: Ép Data từ cTrader chảy sang Python
1. Mở cTrader, bấm biểu tượng `{ }` (Automate) hoặc Editor.
2. Build code (Ctrl + B) cho cả 3 file indicator.
3. Trở lại màn hình Chart cTrader (Ví dụ: Mở cặp EURUSD chart m1). Kéo 3 indicator này vào.
4. **Quan trọng:** Ngay khi bạn thả Indicator vào chart (với `Export History Data = Yes`), cửa sổ Python Console sẽ nhảy liên tục hàng chục nghìn dòng. Đây là bước **Feed AI** - toàn bộ quá khứ đang chảy vào Python.
5. Khi dừng cuộn, hệ thống đã vào trạng thái **Realtime**. Cứ giá chạy (Tick nhảy), Indicator tự cập nhật và bắn JSON về cập nhật State cho AI.

---

## 4. Cách AI phân tích Luồng dữ liệu (State Aggregation)

1. **Gửi độc lập:** 3 indicator hoàn toàn mù nhau, mỗi file tự nhai tự chạy và gói 1 cụm JSON riêng (gắn nhãn `symbol` và `timestamp`).
2. **Gộp State thông minh:** Thay vì phải chờ cả 3, Python `ai_analyzer.process_data()` dùng hàm `update()` để dán đè thay đổi vào `self.symbol_state[symbol]`. 
3. **Trigger:** Hàm check nếu `deltaRank`, `wyckoffVolume` và `vpPOC` cùng lúc tồn tại trong `state`.
4. **Trích Lọc Feature:** Lúc này AI moi ra 1 mảng numpy Data `[delta, wyckoff_wave, poc_distance, tick_vol]` và đưa cho Dummy Model phán.
5. **Vào lệnh MT5:** Giả sử tín hiệu báo Lệnh BÁN MẠNH (Predict -1, Confidence 80%). Code nhảy xuống dòng `mt5_client.place_order_with_risk()`. Tự động cắn Stop Loss = 20 Pips, Take Profit = 40 Pips và RR=2.

---

## 5. Cần làm gì tiếp theo để hoàn thiện hệ thống?

Bộ khung (Pipeline) kết nối Data -> Model -> Trade của dự án đã sẵn sàng 100%. Đây là con đường nâng cấp thành **Sát thủ kiếm tiền** thực thụ:

1. **Export ra CSV để Train:** Hãy viết thêm hàm tự động save cái chuỗi `symbol_state` realtime kia ra đuôi `.csv` (thay vì in ra console mỏi mắt). Sau đó cứ cắm máy lấy khoảng 1 tuần data M1 (Hoặc dùng History để cào ngay lập tức 1 năm data).
2. **Feature Engineering chuyên sâu:** AI hiện tại mới nhắm mắt rút đại 4 cái râu ria bên ngoài (Tổng Delta, 1 giá POC). Nó đang **"Phí phạm thuật toán"** vì nó bỏ rơi toàn bộ mảng Delta Array của dòng Footprint xịn xò. Cần viết lại đoạn `features = np.array(...)` để ép mô hình dùng dữ liệu ở từng ô múc giá.
3. **Thay máu Mô hình học Rừng Bất Định:** Xóa file `model.pkl` đi. Hãy code 1 file Jupyter Notebook rời (`train_ai.ipynb`) dùng TensorFlowLSTM hoặc XGBoost để nạp mảng Footprint Data vào học. Train xong thì Output ra file model mới để cho `ai_analyzer.py` sài.
4. **Uncomment Giao Dịch Thật:** Vào file `ai_analyzer.py`, dòng 91 và 95. Bỏ dấu `#` cho hàm `self.mt5_client.place_order_with_risk(...)` để cho phép AI chính thức vào tiền thật.
