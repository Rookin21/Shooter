using System.Reflection.Emit;
using System.Text;

namespace Shooter
{
    class Program
    {
        private const int _ScreenWidth = 100; // Ширина экрана
        private const int _ScreenHeight = 50; // Высота экрана

        private const int _MapWidth = 32; // Ширина игрового поля
        private const int _MapHeight = 32; // Высота игрового поля

        private const double _Fov = Math.PI / 3; // Поле видимости игрока
        private const double _Depth = 16; // Глубина, до которой игрок будет идти

        // Координаты игрока на карте
        private static double _PlayerX = 5;
        private static double _PlayerY = 5;
        private static double _PlayerA = 0; // Угол, под которым смотрит игрок

        private static readonly StringBuilder _Map = new StringBuilder();

        // Массив символов, который будет обновляться каждую итерацию
        private static readonly char[] _Screen = new char[_ScreenWidth * _ScreenHeight];

        static void Main(string[] args)
        {
            Console.SetWindowSize(_ScreenWidth, _ScreenHeight);  // Устанановка разрешения экрана 
            Console.SetBufferSize(_ScreenWidth, _ScreenHeight); // Установка размера буфера
            Console.CursorVisible = false; // Выключение курсора

            InitMap();

            DateTime DateTimeFrom = DateTime.Now; // Время добавлено для того, чтобы не дергалась анимация           

            while (true)
            {
                var DateTimeTo  = DateTime.Now;
                double ElapsedTime = (DateTimeTo - DateTimeFrom).TotalSeconds;
                DateTimeFrom = DateTime.Now;

                if(Console.KeyAvailable)
                {
                    // Считывание с клавиатуры для движения
                    ConsoleKey consoleKey = Console.ReadKey(intercept: true).Key; 

                    switch (consoleKey)
                    {
                        case ConsoleKey.A: // Движение влево
                            _PlayerA += ElapsedTime * 2;
                            break; 
                        case ConsoleKey.D: // Движение вправо
                            _PlayerA -= ElapsedTime * 2;
                            break;
                        case ConsoleKey.W: // Движение прямо
                            {
                                _PlayerX += Math.Sin(_PlayerA) * 10 * ElapsedTime;
                                _PlayerY += Math.Cos(_PlayerA) * 10 * ElapsedTime;

                                // Если уперлись в стену, то делаем шаг назад
                                if (_Map[(int)_PlayerY * _MapWidth + (int)_PlayerX] == '#')
                                {
                                    _PlayerX -= Math.Sin(_PlayerA) * 10 * ElapsedTime;
                                    _PlayerY -= Math.Cos(_PlayerA) * 10 * ElapsedTime;
                                }
                                break;
                            }
                        case ConsoleKey.S: // Движение назад
                            {
                                _PlayerX -= Math.Sin(_PlayerA) * 10 * ElapsedTime;
                                _PlayerY -= Math.Cos(_PlayerA) * 10 * ElapsedTime;

                                // Если уперлись в стену, то делаем шаг впеед
                                if (_Map[(int)_PlayerY * _MapWidth + (int)_PlayerX] == '#')
                                {
                                    _PlayerX += Math.Sin(_PlayerA) * 10 * ElapsedTime;
                                    _PlayerY += Math.Cos(_PlayerA) * 10 * ElapsedTime;
                                }
                                break;
                            }
                    }
                }

                for (int x = 0; x < _ScreenWidth; x++)
                {
                    double RayAngle = _PlayerA + _Fov / 2 - x * _Fov / _ScreenWidth; // Расчет угла

                    // Для расчета координат игрока
                    double RayX = Math.Sin(RayAngle);
                    double RayY = Math.Cos(RayAngle);

                    double DistanceToWall = 0; // Дистанция до стены
                    bool HitWall = false; // Попали в стену или нет
                    bool IsBound = false; // Попадает в угол или нет

                    while (!HitWall && DistanceToWall < _Depth) // Пока не попали в стену
                    {
                        DistanceToWall += 0.1; // Увеличиваем дистанцию

                        // Показывают координаты до откуда дошел луч
                        int TestX = (int)(_PlayerX + RayX * DistanceToWall);
                        int TestY = (int)(_PlayerY + RayY * DistanceToWall);

                        // Проверка не выхода за границы карты и глубины
                        if (TestX < 0 || TestX >= _Depth + _PlayerX || TestY < 0 || TestY >= _Depth + _PlayerY)
                        {
                            HitWall = true;
                            DistanceToWall = _Depth;
                        }
                        else // луч попадает в стену
                        {
                            char TestCell = _Map[TestY * _MapWidth + TestX];

                            if (TestCell == '#')
                            {
                                HitWall = true;

                                // Отрисовка граней стены
                                var BoundVecorList = new List<(double module, double cos)>(); 
                                
                                // Проход по 4 ребрам
                                for(int tx = 0; tx < 2; tx++)
                                {
                                    for (int ty = 0; _PlayerY < 2; ty++)
                                    {
                                        // Подсчет x и y объекта
                                        double vx = TestX + tx - _PlayerX;
                                        double vy = TestY + ty - _PlayerY;

                                        // Подсчет модуль вектора
                                        double VectorModule = Math.Sqrt(vx * vx + vy * vy);
                                        // Подсчет косинуса угла
                                        double CosAngle = RayX * vx / VectorModule + RayY * vy / VectorModule;  

                                        BoundVecorList.Add((VectorModule, CosAngle)); // Добавление списка
                                    }
                                }
                                // Сортировка списка по модулю, чтобы первые были два самых ближайших вектора
                                BoundVecorList = BoundVecorList.OrderBy(v => v.module).ToList();

                                // Деление нужно для того, чтобы издали грани отображались корректно
                                double BoundAngle = 0.03 / DistanceToWall; 

                                // Проверка для нулевого и первого элемента, попадает ли угол в константу
                                if (Math.Acos(BoundVecorList[0].cos) < BoundAngle ||
                                    Math.Acos(BoundVecorList[1].cos) < BoundAngle)
                                    IsBound = true;
                            }
                            else
                            {
                                _Map[TestY * _MapWidth + TestX] = '*';
                            }
                        }
                    }

                    int Ceiling = (int)(_ScreenHeight / 2d - _ScreenHeight * _Fov / DistanceToWall); // Расчет потолка
                    int Floor = _ScreenHeight - Ceiling; // Расчет пола

                    char WallShade; // Окрас стены

                    if (IsBound)
                        WallShade = '|';
                    else if (DistanceToWall < _Depth / 4d)
                        WallShade = '\u2588';
                    else if (DistanceToWall < _Depth / 3d)
                        WallShade = '\u2593';
                    else if (DistanceToWall < _Depth / 2d)
                        WallShade = '\u2592';
                    else if (DistanceToWall < _Depth)
                        WallShade = '\u2591';
                    else
                        WallShade = ' ';

                    // Цикл, который проходит по высоте экрана
                    for (int y = 0; y < _ScreenHeight; y++)
                    {
                        if (y <= Ceiling) // Если потолок
                        {
                            _Screen[y * _ScreenWidth + x] = ' '; // Отрисовываем пустой символ
                        }
                        else if (y > Ceiling && y <= Floor) // Если больше, чем потолок и меньше, чем пол, то это стена
                        {
                            _Screen[y * _ScreenWidth + x] = WallShade; // Отрисовка стены
                        }
                        else // Декорирование пола. Взависимости от приближения
                        {
                            char FloorShade;

                            double b = 1 - (y - _ScreenHeight / 2d) / (_ScreenHeight / 2d);

                            if (b < 0.25)
                                FloorShade = '#';
                            else if (b < 0.5)
                                FloorShade = 'x';
                            else if (b < 0.75)
                                FloorShade = '-';
                            else if (b < 0.9)
                                FloorShade = '.';
                            else
                                FloorShade = ' ';

                            _Screen[y * _ScreenWidth + x] = FloorShade; // Отрисововка пола
                        }
                    }
                }

                // Вывод информации в углу экрана
                char[] stats = $"X: {_PlayerX}, Y: {_PlayerY}, A: {_PlayerA}, FPS: {(int)(1 / ElapsedTime)}"
                    .ToArray();
                stats.CopyTo(array:_Screen, index: 0);

                // Вывод карты
                for (int x = 0; x < _MapWidth; x++)
                {
                    for (int y = 0; y < _MapHeight; y++)
                    {
                        _Screen[(y + 1) * _ScreenWidth + x] = _Map[y * _MapWidth+ x];
                    }
                }

                // Положение игрока на карте
                _Screen[(int)(_PlayerY + 1) * _ScreenWidth + (int)_PlayerX] = 'P';

                Console.SetCursorPosition(left: 0, top: 0);
                Console.Write(buffer: _Screen);
            }
        }

        private static void InitMap()
        {
            _Map.Clear();
            // Стена 
            _Map.Append("#################################"); // Верх стены
            _Map.Append("#....................#..........#"); // Точки пустое пространство
            _Map.Append("#....................#..........#");
            _Map.Append("#....................#..........#");
            _Map.Append("#....................#..........#");
            _Map.Append("#....................#..........#");
            _Map.Append("#....................#..........#");
            _Map.Append("#....................#..........#");
            _Map.Append("#......######........#..........#");
            _Map.Append("#....................#..........#");
            _Map.Append("#...............................#");
            _Map.Append("#...............................#");
            _Map.Append("#.....................###########");
            _Map.Append("#........#......................#");
            _Map.Append("#...............................#");
            _Map.Append("#...............................#");
            _Map.Append("#.........########..............#");
            _Map.Append("#...............................#");
            _Map.Append("#...............................#");
            _Map.Append("#...............................#");
            _Map.Append("#...............................#");
            _Map.Append("#...............................#");
            _Map.Append("###########.....................#");
            _Map.Append("#...............................#");
            _Map.Append("#.........#.....................#");
            _Map.Append("#.........#.....................#");
            _Map.Append("#.........#.....................#");
            _Map.Append("#.........#.....................#");
            _Map.Append("#.........#.....................#");
            _Map.Append("#.........#.....................#");
            _Map.Append("#.........#.....................#");
            _Map.Append("#################################"); // Низ стены
        }
    }
}

