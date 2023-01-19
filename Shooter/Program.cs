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

        private static string _Map = ""; // Для хранения самой карты

        // Массив символов, который будет обновляться каждую итерацию
        private static readonly char[] _Screen = new char[_ScreenWidth * _ScreenHeight];

        static void Main(string[] args)
        {
            Console.SetWindowSize(_ScreenWidth, _ScreenHeight);  // Устанановка разрешения экрана 
            Console.SetBufferSize(_ScreenWidth, _ScreenHeight); // Установка размера буфера
            Console.CursorVisible = false; // Выключение курсора

            // Стена 
            _Map += "#################################"; // Верх стены
            _Map += "#...............................#"; // Точки пустое пространство
            _Map += "#...............................#";
            _Map += "#...............................#";
            _Map += "#...............................#";
            _Map += "#...............................#";
            _Map += "#...............................#";
            _Map += "#...............................#";
            _Map += "#...............................#";
            _Map += "#...............................#";
            _Map += "#...............................#";
            _Map += "#...............................#";
            _Map += "#...............................#";
            _Map += "#...............................#";
            _Map += "#...............................#";
            _Map += "#...............................#";
            _Map += "#...............................#";
            _Map += "#...............................#";
            _Map += "#...............................#";
            _Map += "#...............................#";
            _Map += "#...............................#";
            _Map += "#...............................#";
            _Map += "#...............................#";
            _Map += "#...............................#";
            _Map += "#...............................#";
            _Map += "#...............................#";
            _Map += "#...............................#";
            _Map += "#...............................#";
            _Map += "#...............................#";
            _Map += "#...............................#";
            _Map += "#...............................#";
            _Map += "#################################"; // Низ стены

            while (true)
            {
                // Каждую итерацию добавляем угол
                _PlayerA += 0.005;

                for (int x = 0; x < _ScreenWidth; x++)
                {
                    double RayAngle = _PlayerA + _Fov / 2 - x * _Fov / _ScreenWidth; // Расчет угла

                    // Для расчета координат игрока
                    double RayX = Math.Sin(RayAngle);
                    double RayY = Math.Cos(RayAngle);

                    double DistanceToWall = 0; // Дистанция до стены
                    bool HitWall = false; // Попали в стену или нет

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
                            }
                        }
                    }

                    int Ceiling = (int)(_ScreenHeight / 2d - _ScreenHeight * _Fov / DistanceToWall); // Расчет потолка
                    int Floor = _ScreenHeight - Ceiling; // Расчет пола

                    char WallShade; // Окрас стены

                    if (DistanceToWall < _Depth / 4d)
                    {
                        WallShade = '\u2588';
                    }
                    else if (DistanceToWall < _Depth / 3d)
                    {
                        WallShade = '\u2593';
                    }
                    else if (DistanceToWall < _Depth / 2d)
                    {
                        WallShade = '\u2592';
                    }
                    else if (DistanceToWall < _Depth)
                    {
                        WallShade = '\u2591';
                    }
                    else
                    {
                        WallShade = ' ';
                    }

                    // Цикл, который проходит по высоте экрана
                    for (int y = 0; y < _ScreenHeight; y++)
                    {
                        if (y <= Ceiling) // Если потолок
                        {
                            _Screen[y * _ScreenWidth + x] = ' '; // Отрисовываем пустой символ
                        }
                        else if (y > Ceiling && y <= Floor) // Если больше, чем потолок и меньше, чем пол, то это стена
                        {
                            _Screen[y * _ScreenWidth + x] = WallShade; // Отрисовываем стену
                        }
                        else
                        {
                            _Screen[y * _ScreenWidth + x] = '.'; // Отрисовываем пол
                        }
                    }
                }

                Console.SetCursorPosition(left: 0, top: 0);
                Console.Write(buffer: _Screen);
            }
        }
    }
}

