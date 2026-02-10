# CoilMetalStore
backend для работы со складом рулонов металла
#### для запуска 
1 собрать решение
2 выполнить docker-compose up -d --build
3 открыть http://localhost:8081
#### Add
добавляет новый рулон
http://localhost:8081/StoreManagement/Add?length=1&weight=2
#### GetCoils
получение списка рулонов по фильтрам: MinId, MaxId, MinLength, MaxLength, MinDateAdd, MaxDateAdd, MinDateDelete, MaxDateDelete
https://localhost:8081/StoreManagement/ПetСoilss?MinВateФвв=2026-02-07&MinLength=1
#### Delete
удаление рулона со склада по id
https://localhost:8081/StoreManagement/delete?id=3
#### GetStatistics
получение статистики за выбранный период
https://localhost:8081/StoreManagement/getstatistics?MinDate=2026-02-7&MaxDate=2026-02-12
