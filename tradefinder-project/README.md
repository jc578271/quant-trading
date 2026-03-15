# Tradefinder Project (Recreated)

Project này được tạo ra từ mã nguồn dịch ngược (decompiled) của `tradefinder-3.7.2.jar`.

## Cấu trúc Project
- `src/main/java/ttw`: Chứa mã nguồn đã dịch ngược.
- `build.gradle`: File cấu hình build (Java 17+, dependencies: Bookmap API, OkHttp, GSON).

## Hướng dẫn Build
Để build thử, bạn có thể chạy lệnh sau trong thư mục này:
```powershell
$env:JAVA_HOME = "C:\Users\hoang\.jdks\openjdk-25.0.2"; .\gradlew compileJava
```

## Tình trạng hiện tại (Lưu ý quan trọng)
Quá trình build hiện tại sẽ **THẤT BẠI** với rất nhiều lỗi cú pháp (Syntax Errors). Nguyên nhân là do mã nguồn gốc đã bị obfuscate (làm rối):
1. **Trùng tên biến và từ khóa**: Trình dịch ngược tạo ra các tên như `if`, `a`, `b`... vốn là từ khóa hoặc gây xung đột trong Java.
2. **Cấu trúc lệnh bị hỏng**: Một số đoạn mã không thể dịch ngược hoàn chỉnh, tạo ra các lệnh sai cú pháp như `if a3;` hoặc `if if_ = a3;`.

## Cách khắc phục
Để project có thể chạy được, bạn cần:
1. Dùng tính năng **Refactor -> Rename** trong IDE (như IntelliJ) để đổi tên các Class/Variable bị trùng hoặc vi phạm từ khóa.
2. Sửa thủ công các dòng code bị lỗi cấu trúc (dựa trên logic xung quanh).
3. Bổ sung thêm các thư viện còn thiếu nếu phát hiện lỗi "Symbol not found".
