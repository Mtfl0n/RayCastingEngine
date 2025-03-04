using SFML.Graphics;
using SFML.System;
using SFML.Window;

namespace Raycaster
{
    public static class Rays
    {
        // Поле зрения (FOV) в радианах, примерно 60 градусов
        private static float fov = 1.0472f;
        
        // Половина поля зрения для симметричного распределения лучей
        private static float halfFov = fov / 2f;
        
        // Угловой шаг между соседними лучами, зависит от ширины экрана
        private static float increment = fov / Program.SCREEN_WIDTH;
        
        // Середина экрана по высоте, где будет линия горизонта
        private static int halfScreen = Program.SCREEN_HEIGHT / 2;
        
        // Максимальное количество шагов для луча, чтобы ограничить дальность видимости
        private static int maxSteps = 8;
        
        // 360 градусов в радианах для нормализации углов
        private static float twoPi = MathF.PI * 2f;
        
        // 270 градусов в радианах для определения направлений лучей
        private static float halfThreePi = (3f * MathF.PI) / 2f;
        
        // 90 градусов в радианах для проверки направлений
        private static float halfPi = MathF.PI / 2f;
        
        // Размеры текстуры по X и Y (512x512 пикселей)
        private const float TEXTURE_SIZE_X = 512f;
        private const float TEXTURE_SIZE_Y = 512f;
        
        // Цвет неба (фиолетовый)
        private static Color skyColor = new Color(135, 106, 235, 255);
        
        // Цвет пола (серый)
        private static Color groundColor = new Color(64, 64, 64);

        // Метод для вычисления расстояния между двумя точками
        public static float GetDistance(Vector2f initPos, Vector2f endPos)
        {
            // Евклидова формула: sqrt((x2 - x1)^2 + (y2 - y1)^2)
            return MathF.Sqrt(MathF.Pow(endPos.X - initPos.X, 2f) + MathF.Pow(endPos.Y - initPos.Y, 2f));
        }

        // Метод для поиска пересечения луча с вертикальными стенами (по оси X)
        public static Vector2f GetRayXHit(Vector2f playerPos, float angle, Map map)
        {
            float rayAngle = angle; // Угол луча
            // Наклон перпендикулярной линии к лучу
            float perpendicularSlope = -1 / MathF.Tan(rayAngle);

            Vector2f rayPos = new Vector2f(); // Текущая позиция луча
            Vector2f rayOffset = new Vector2f(); // Шаг луча
            Vector2i mapCoords = new Vector2i(); // Координаты на карте

            if (rayAngle > MathF.PI) // Если луч идёт вниз
            {
                // Находим ближайшую вертикальную линию сетки сверху
                rayPos.Y = (((int)(playerPos.Y) / Tile.TILESIZE_Y) * Tile.TILESIZE_Y) - 0.0001f;
                // Вычисляем X через уравнение прямой
                rayPos.X = (playerPos.Y - rayPos.Y) * perpendicularSlope + playerPos.X;
                rayOffset.Y = -Tile.TILESIZE_Y; // Шаг вверх по Y
                rayOffset.X = -(rayOffset.Y) * perpendicularSlope; // Соответствующий шаг по X
            }
            else if (rayAngle < MathF.PI) // Если луч идёт вверх
            {
                // Находим ближайшую вертикальную линию сетки снизу
                rayPos.Y = (((int)(playerPos.Y) / Tile.TILESIZE_Y) * Tile.TILESIZE_Y) + Tile.TILESIZE_Y;
                rayPos.X = (playerPos.Y - rayPos.Y) * perpendicularSlope + playerPos.X;
                rayOffset.Y = Tile.TILESIZE_Y; // Шаг вниз по Y
                rayOffset.X = -(rayOffset.Y) * perpendicularSlope; // Шаг по X
            }
            else if (rayAngle == 0 || rayAngle == MathF.PI) // Если луч горизонтальный
            {
                rayPos.X = playerPos.X;
                rayPos.Y = playerPos.Y; // Луч не пересекает вертикальные стены
            }

            int maxDistance = maxSteps; // Ограничение на количество шагов
            while (maxDistance > 0)
            {
                // Переводим позицию луча в координаты карты
                mapCoords.X = (int)(rayPos.X) / Tile.TILESIZE_Y;
                mapCoords.Y = ((int)(rayPos.Y) / Tile.TILESIZE_Y);

                // Если вышли за пределы карты, выходим
                if (mapCoords.X < 0 || mapCoords.Y < 0 || mapCoords.X > map.Size.X - 1 || mapCoords.Y > map.Size.Y - 1)
                    break;
                // Если наткнулись на стену, выходим
                if (map.WorldMap[mapCoords.X, mapCoords.Y] != 0)
                    break;
                else
                {
                    // Двигаем луч на один шаг
                    rayPos.X += rayOffset.X;
                    rayPos.Y += rayOffset.Y;
                }
                maxDistance--; // Уменьшаем счётчик
            }
            return rayPos; // Возвращаем точку пересечения
        }

        // Метод для поиска пересечения луча с горизонтальными стенами (по оси Y)
        public static Vector2f GetRayYHit(Vector2f playerPos, float angle, Map map)
        {
            float rayAngle = angle; // Угол луча
            float perpendicularSlope = -MathF.Tan(rayAngle); // Наклон для горизонтальных пересечений

            Vector2f rayPos = new Vector2f(); // Текущая позиция луча
            Vector2f rayOffset = new Vector2f(); // Шаг луча
            Vector2i mapCoords = new Vector2i(); // Координаты на карте

            if (rayAngle > halfPi && rayAngle < halfThreePi) // Если луч идёт влево
            {
                // Находим ближайшую горизонтальную линию сетки слева
                rayPos.X = (((int)(playerPos.X) / Tile.TILESIZE_X) * Tile.TILESIZE_X) - 0.0001f;
                rayPos.Y = (playerPos.X - rayPos.X) * perpendicularSlope + playerPos.Y;
                rayOffset.X = -Tile.TILESIZE_X; // Шаг влево по X
                rayOffset.Y = -(rayOffset.X) * perpendicularSlope; // Шаг по Y
            }
            else if (rayAngle < halfPi || rayAngle > halfThreePi) // Если луч идёт вправо
            {
                // Находим ближайшую горизонтальную линию сетки справа
                rayPos.X = (((int)(playerPos.X) / Tile.TILESIZE_X) * Tile.TILESIZE_X) + Tile.TILESIZE_X;
                rayPos.Y = (playerPos.X - rayPos.X) * perpendicularSlope + playerPos.Y;
                rayOffset.X = Tile.TILESIZE_X; // Шаг вправо по X
                rayOffset.Y = -(rayOffset.X) * perpendicularSlope; // Шаг по Y
            }
            else if (rayAngle == 0 || rayAngle == halfPi) // Если луч вертикальный
            {
                rayPos.Y = playerPos.Y;
                rayPos.X = playerPos.X; // Исправлено: должно быть playerPos.X
            }

            int maxDistance = maxSteps; // Ограничение на количество шагов
            while (maxDistance > 0)
            {
                mapCoords.X = (int)(rayPos.X) / Tile.TILESIZE_X; // Переводим X в координаты карты
                mapCoords.Y = ((int)(rayPos.Y) / Tile.TILESIZE_X); // Переводим Y в координаты карты (возможно, здесь нужен Tile.TILESIZE_Y)

                if (mapCoords.X < 0 || mapCoords.Y < 0 || mapCoords.X > map.Size.X - 1 || mapCoords.Y > map.Size.Y - 1)
                    break; // Если вышли за пределы карты, выходим
                if (map.WorldMap[mapCoords.X, mapCoords.Y] != 0)
                    break; // Если наткнулись на стену, выходим
                else
                {
                    rayPos.X += rayOffset.X; // Двигаем луч по X
                    rayPos.Y += rayOffset.Y; // Двигаем луч по Y
                }
                maxDistance--; // Уменьшаем счётчик шагов
            }
            return rayPos; // Возвращаем точку пересечения
        }

        // Метод для отрисовки 3D-мира без текстур
        public static void Draw3DWorld(Player player, RenderWindow window, Map map)
        {
            Color blue = new Color(0, 0, 200, 255); // Базовый цвет стен
            for (int rayNum = 0; rayNum < Program.SCREEN_WIDTH; rayNum++)
            {
                // Вычисляем угол луча
                float angle = player.Angle - halfFov + rayNum * increment;
                if (angle < 0)
                    angle += MathF.PI * 2f; // Нормализуем угол, если он меньше 0
                if (angle > MathF.PI * 2f)
                    angle -= MathF.PI * 2f; // Нормализуем угол, если он больше 360 градусов

                // Находим пересечения с вертикальными и горизонтальными стенами
                Vector2f rayX = GetRayXHit(player.Position, angle, map);
                Vector2f rayY = GetRayYHit(player.Position, angle, map);

                // Расстояния до пересечений
                float distanceX = GetDistance(player.Position, rayX);
                float distanceY = GetDistance(player.Position, rayY);

                float finalDistance = float.MaxValue; // Изначально большое значение для поиска минимума
                if (distanceY < distanceX)
                {
                    blue = new Color(0, 0, 100, 255); // Темнее для горизонтальных стен
                    finalDistance = distanceY; // Устанавливаем расстояние до горизонтальной стены
                }
                if (distanceX < distanceY)
                {
                    finalDistance = distanceX; // Устанавливаем расстояние до вертикальной стены
                    blue = new Color(0, 0, 150, 255); // Светлее для вертикальных стен
                }

                // Коррекция "рыбьего глаза" для реалистичной перспективы
                finalDistance = finalDistance * MathF.Cos(angle - player.Angle);

                // Высота стены на экране, зависит от расстояния
                float wallHeight = MathF.Floor(halfScreen / finalDistance * 25f);

                // Вершины для стены
                Vertex[] wallStrip = {
                    new Vertex(new Vector2f(rayNum, halfScreen - wallHeight), blue), // Верх стены
                    new Vertex(new Vector2f(rayNum, halfScreen + wallHeight), blue)  // Низ стены
                };
                // Вершины для неба
                Vertex[] sky =
                {
                    new Vertex(new Vector2f(rayNum, halfScreen + wallHeight), new Color(135, 106, 125, 255)), // Нижняя граница неба
                    new Vertex(new Vector2f(rayNum, 0f), new Color(135, 106, 235, 255)) // Верхняя граница неба
                };
                // Вершины для пола
                Vertex[] floor = {
                    new Vertex(new Vector2f(rayNum, halfScreen + wallHeight), new Color(64, 64, 64)), // Верхняя граница пола
                    new Vertex(new Vector2f(rayNum, Program.SCREEN_HEIGHT), new Color(32, 32, 32)) // Нижняя граница пола
                };

                // Рисуем небо, стену и пол
                window.Draw(sky, PrimitiveType.Lines);
                window.Draw(wallStrip, PrimitiveType.Lines);
                window.Draw(floor, PrimitiveType.Lines);
            }
        }

        // Метод для отрисовки 3D-мира с текстурами
        public static void Draw3DWorldTextured(Player player, RenderWindow window, Map map, VertexArray wallVA, VertexArray ceilingFloorVA)
        {
            for (int rayNum = 0; rayNum < Program.SCREEN_WIDTH; rayNum++)
            {
                float angle = player.Angle - halfFov + rayNum * increment; // Угол текущего луча
                if (angle < 0)
                    angle += twoPi; // Нормализуем угол, если меньше 0
                if (angle > twoPi)
                    angle -= twoPi; // Нормализуем угол, если больше 360 градусов

                Vector2f rayX = GetRayXHit(player.Position, angle, map); // Пересечение с вертикальными стенами
                Vector2f rayY = GetRayYHit(player.Position, angle, map); // Пересечение с горизонтальными стенами
                Vector2f finalPos = new Vector2f(); // Конечная позиция луча

                float distanceX = GetDistance(player.Position, rayX); // Расстояние до вертикальной стены
                float distanceY = GetDistance(player.Position, rayY); // Расстояние до горизонтальной стены

                float finalDistance = 0; // Итоговое расстояние до ближайшей стены
                int textureX = 0; // Координата X на текстуре
                Color brightness = Color.White; // Яркость стены (для затемнения)
                int side = 0; // 0 - вертикальная стена, 1 - горизонтальная
                if (distanceY < distanceX)
                {
                    finalPos = rayY; // Ближайшая точка - горизонтальная стена
                    finalDistance = distanceY; // Устанавливаем расстояние
                    // Вычисляем координату текстуры для горизонтальной стены
                    textureX = (int)(finalPos.Y * (TEXTURE_SIZE_X / Tile.TILESIZE_X) % TEXTURE_SIZE_X);
                    side = 1; // Указываем, что это горизонтальная стена
                }
                if (distanceX < distanceY)
                {
                    finalDistance = distanceX; // Устанавливаем расстояние до вертикальной стены
                    finalPos = rayX; // Ближайшая точка - вертикальная стена
                    brightness = new Color(200, 200, 200, 255); // Затемнение для вертикальных стен
                    // Вычисляем координату текстуры для вертикальной стены
                    textureX = (int)(finalPos.X * (TEXTURE_SIZE_X / Tile.TILESIZE_X) % TEXTURE_SIZE_X);
                }
                // Координаты на карте для определения текстуры
                Vector2i mapCoords = new Vector2i((int)(finalPos.X) / Tile.TILESIZE_Y, ((int)(finalPos.Y) / Tile.TILESIZE_Y));

                // Коррекция расстояния для устранения эффекта "рыбьего глаза"
                finalDistance = finalDistance * MathF.Cos(angle - player.Angle);

                // Высота стены на экране
                float wallHeight = MathF.Floor(halfScreen / finalDistance * 100f);
                float groundPixel = (int)(wallHeight + halfScreen); // Пиксель низа стены
                float ceilingPixel = (int)(-wallHeight + halfScreen); // Пиксель верха стены

                // ID текстуры из карты (0 - нет стены, 1+ - текстуры)
                int textureID = map.WorldMap[mapCoords.X, mapCoords.Y] - 1;

                // Вершины для стены с текстурой
                wallVA.Append(new Vertex(
                    position: new Vector2f(rayNum, ceilingPixel), // Позиция верха стены
                    color: brightness, // Цвет с учётом яркости
                    texCoords: new Vector2f(textureX + textureID * TEXTURE_SIZE_X, 0))); // Координаты текстуры (верх)

                wallVA.Append(new Vertex(
                    position: new Vector2f(rayNum, groundPixel), // Позиция низа стены
                    color: brightness, // Цвет с учётом яркости
                    texCoords: new Vector2f(textureX + textureID * TEXTURE_SIZE_X, TEXTURE_SIZE_Y))); // Координаты текстуры (низ)

                // Вершины для пола
                ceilingFloorVA.Append(new Vertex(new Vector2f(rayNum, groundPixel), groundColor)); // Верх пола
                ceilingFloorVA.Append(new Vertex(new Vector2f(rayNum, Program.SCREEN_HEIGHT), groundColor)); // Низ пола

                // Вершины для неба
                ceilingFloorVA.Append(new Vertex(new Vector2f(rayNum, ceilingPixel), skyColor)); // Низ неба
                ceilingFloorVA.Append(new Vertex(new Vector2f(rayNum, 0f), skyColor)); // Верх неба
            }
        }

        // Метод для получения ближайшей точки пересечения луча
        public static Vector2f GetFinalRay(Player player, float angle, Map map)
        {
            Vector2f rayPosY = GetRayYHit(player.Position, angle, map); // Пересечение с горизонтальными стенами
            Vector2f rayPosX = GetRayXHit(player.Position, angle, map); // Пересечение с вертикальными стенами
            float distanceX = GetDistance(player.Position, rayPosX); // Расстояние до вертикальной стены
            float distanceY = GetDistance(player.Position, rayPosY); // Расстояние до горизонтальной стены
            if (distanceX < distanceY)
            {
                return rayPosX; // Возвращаем ближайшую вертикальную точку
            }
            else
            {
                return rayPosY; // Возвращаем ближайшую горизонтальную точку
            }
        }

        // Метод для отрисовки лучей на миникарте
        public static void DrawMinimapRays(Player player, RenderWindow window, Map map)
        {
            for (int rayNum = 0; rayNum < Program.SCREEN_WIDTH; rayNum++)
            {
                float angle = player.Angle - halfFov + rayNum * increment; // Угол текущего луча
                if (angle < 0)
                    angle += MathF.PI * 2f; // Нормализуем угол, если меньше 0
                if (angle > MathF.PI * 2f)
                    angle -= MathF.PI * 2f; // Нормализуем угол, если больше 360 градусов
                Vector2f rayPos = GetFinalRay(player, angle, map); // Получаем точку пересечения луча
                Vertex[] wallStrip = new Vertex[2];
                wallStrip[0] = new Vertex(new Vector2f(player.Position.X, player.Position.Y), Color.Blue); // Начало луча (позиция игрока)
                wallStrip[1] = new Vertex(new Vector2f(rayPos.X, rayPos.Y), Color.Blue); // Конец луча (точка пересечения)
                window.Draw(wallStrip, PrimitiveType.Lines); // Рисуем луч на миникарте
            }
        }

        // Метод для перевода радиан в градусы
        public static float RadToDeg(float rad)
        {
            return (180 / MathF.PI) * rad; // Формула перевода: радианы * (180 / PI)
        }
    }
}