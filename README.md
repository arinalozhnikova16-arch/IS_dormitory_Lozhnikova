# IS_dormitory_Lozhnikova

# ИС «Управление студенческим общежитием» (C# / .NET MAUI)

Учебный прототип кроссплатформенного (в первую очередь, мобильного) приложения для коменданта и проживающих студентов. Автоматизирует расселение, учёт заявок на ремонт и строит базовую аналитику по загруженности общежития. Проект выполнен в рамках контрольной точки по дисциплине "Объектно-ориентированный анализ и программирование".

## Что делает приложение
- **Учёт и умное расселение.** Три связанные сущности (Комнаты, Проживающие, Заявки). При заселении/выселении студента счётчик занятых мест в комнате обновляется автоматически. Реализовано модальное окно для выбора комнаты с учётом пола, курса и факультета.
- **Заявки на ремонт.** Создание и отслеживание заявок. Есть переключатель для просмотра только своих обращений (истории текущего пользователя).
- **Аналитика** Отдельные экраны со статистикой (LiveCharts): загрузка койко-мест, распределение жильцов по факультетам и курсам, динамика и статусы заявок на ремонт.
- **Главный экран.** Встроенная геолокация, быстрый звонок и копирование email.

## Использованный технологический стек
- C# / .NET MAUI
- SQLite + Entity Framework Core (локальная БД)
- LiveCharts (визуализация)

## Примеры работы приложения
### Пользовательские формы
<img width="333" height="739" alt="image" src="https://github.com/user-attachments/assets/5cf483b0-0baa-4af8-abdd-7a805dbcf4f2" />
<img width="263" height="739" alt="image" src="https://github.com/user-attachments/assets/7406bac5-560a-4aab-b0c1-a51e5c8e4b06" />
<img width="229" height="739" alt="image" src="https://github.com/user-attachments/assets/060b2424-8c6e-4051-9289-49e86ba4cb34" />
<img width="333" height="740" alt="image" src="https://github.com/user-attachments/assets/a7578da4-6815-47fe-9080-3ad3c4b2a8fb" />
<img width="331" height="735" alt="image" src="https://github.com/user-attachments/assets/a65ba394-894e-4078-b81a-92c445520f9d" />

### Аналитика
<img width="336" height="747" alt="image" src="https://github.com/user-attachments/assets/029ee6cc-c83b-4f08-ab16-84270cc94d3d" />
<img width="330" height="733" alt="image" src="https://github.com/user-attachments/assets/e17ff89f-dc36-49e1-8052-96c1ba26cb26" />

## Главное меню
<img width="233" height="519" alt="image" src="https://github.com/user-attachments/assets/7d087f6a-6316-42f2-8327-6d7cb3766094" />
<img width="173" height="520" alt="image" src="https://github.com/user-attachments/assets/e86e955f-3618-4976-91f7-a9511570e32b" />
