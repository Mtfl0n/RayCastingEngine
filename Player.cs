using SFML.Graphics;
using SFML.System;
using SFML.Window;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Raycaster
{
    public class Player
    {
        private static Vector2f Size = new Vector2f(6, 6); // Размер игрока (16x16 пикселей)
        private Vector2f position; // Текущая позиция игрока
        Sprite Sprite { get; set; } // Спрайт игрока для отображения
        public Vector2f Velocity { get; set; } // Вектор скорости (направление движения)
        public Vector2i MousePrevious { get; set; } // Предыдущая позиция мыши для управления
        public float Speed { get; set; } // Скорость движения игрока
        public bool WPressed { get; set; } // Флаг нажатия W (вперёд)
        public bool SPressed { get; set; } // Флаг нажатия S (назад)
        public bool APressed { get; set; } // Флаг нажатия A (влево)
        public bool DPressed { get; set; } // Флаг нажатия D (вправо)

        private List<Texture> shotgunTextures; // Список всех текстур шотгана
        private Sprite shotgunSprite; // Спрайт для отображения шотгана
        public bool IsFiring { get; set; } = false; // Флаг выстрела
        public float FiringTimer { get; set; } = 0f; // Таймер анимации
        public int CurrentFrame { get; set; } = 0; // Текущий кадр анимации
        public const float ANIMATION_DURATION = 0.5f; // Общая длительность анимации (в секундах)
        public const int TOTAL_FRAMES = 5; // Количество кадров анимации

        public Vector2f Position
        {
            get { return position; }
            set
            {
                this.position = value;
                if (this.Sprite != null)
                    this.Sprite.Position = value; // Обновляем позицию спрайта
            }
        }

        public Vector2f Direction { get; set; } // Направление взгляда игрока
        private float angle; // Угол поворота игрока в радианах

        public float Angle
        {
            get { return this.angle; }
            set
            {
                if (this.Sprite != null)
                    this.Sprite.Rotation = Rays.RadToDeg(value - MathF.PI / 2); // Поворачиваем спрайт
                this.angle = value;
                this.Direction =
                    new Vector2f(MathF.Cos(this.Angle), MathF.Sin(this.Angle)); // Обновляем вектор направления
            }
        }

        public Player(Vector2f position)
        {
            shotgunTextures = new List<Texture>()
            {
                new Texture(
                    "C:\\Code\\RaycastingEngine\\RayCastingEngineProject\\Raycaster\\resources\\textures\\sprites\\weapon\\shotgun\\0.png"),
                new Texture(
                    "C:\\Code\\RaycastingEngine\\RayCastingEngineProject\\Raycaster\\resources\\textures\\sprites\\weapon\\shotgun\\1.png"),
                new Texture(
                    "C:\\Code\\RaycastingEngine\\RayCastingEngineProject\\Raycaster\\resources\\textures\\sprites\\weapon\\shotgun\\2.png"),
                new Texture(
                    "C:\\Code\\RaycastingEngine\\RayCastingEngineProject\\Raycaster\\resources\\textures\\sprites\\weapon\\shotgun\\3.png"),
                new Texture(
                    "C:\\Code\\RaycastingEngine\\RayCastingEngineProject\\Raycaster\\resources\\textures\\sprites\\weapon\\shotgun\\4.png"),
                new Texture(
                    "C:\\Code\\RaycastingEngine\\RayCastingEngineProject\\Raycaster\\resources\\textures\\sprites\\weapon\\shotgun\\5.png"),
            };
            shotgunSprite = new Sprite(shotgunTextures[0]);
            shotgunSprite.Origin =
                new Vector2f(shotgunTextures[0].Size.X / 2, shotgunTextures[0].Size.Y); // Центр снизу
            shotgunSprite.Scale = new Vector2f(0.5f, 0.5f);

            this.Sprite =
                new Sprite(new Texture(
                    "C:\\Code\\RaycastingEngine\\RayCastingEngineProject\\Raycaster\\bin\\Debug\\net6.0\\resources\\player.png")); // Загружаем текстуру игрока
            this.Sprite.Origin = new Vector2f(Player.Size.X / 2, Player.Size.Y / 2); // Центр спрайта
            this.Position = position; // Устанавливаем начальную позицию
            this.MousePrevious =
                new Vector2i(Program.SCREEN_WIDTH / 2, Program.SCREEN_HEIGHT / 2); // Начальная позиция мыши
            this.Angle = 0; // Начальный угол
            this.Speed = 50f; // Начальная скорость
        }

        public void Draw(RenderWindow window)
        {
            window.Draw(this.Sprite); // Рисуем игрока
        }

        public void StartFiring()
        {
            if (!IsFiring)
            {
                IsFiring = true;
                FiringTimer = ANIMATION_DURATION;
                CurrentFrame = 1; // Переключаемся на кадр выстрела
                shotgunSprite.Texture = shotgunTextures[CurrentFrame];
            }
        }

        public void DrawShotgun(RenderWindow window)
        {
            // Рассчитываем смещение для отдачи
            float totalFiringTime = ANIMATION_DURATION; // Длительность всей анимации
            float offsetY = 0;
            if (IsFiring)
            {
                float t = 1 - FiringTimer / totalFiringTime; // Нормализованное время (0 до 1)
                if (t < 0.5f)
                {
                    offsetY = -40 * t; // Поднимаем вверх в первой половине анимации
                }
                else
                {
                    offsetY = -40 * (1 - t); // Возвращаемся вниз во второй половине
                }
            }

            // Позиция шотгана (центр снизу экрана с учётом отдачи)
            shotgunSprite.Position = new Vector2f(Program.SCREEN_WIDTH / 2, Program.SCREEN_HEIGHT + offsetY);
            window.Draw(shotgunSprite);
        }

        public void UpdatePosition(float deltaTime, Map map)
        {
            if (IsFiring)
            {
                FiringTimer -= deltaTime;
                if (FiringTimer <= 0)
                {
                    IsFiring = false;
                    CurrentFrame = 0;
                }
                else
                {
                    float frameTime = ANIMATION_DURATION / (TOTAL_FRAMES - 1);
                    int newFrame = (int)(FiringTimer / frameTime);
                    if (newFrame != CurrentFrame && newFrame >= 0 && newFrame < TOTAL_FRAMES)
                    {
                        CurrentFrame = newFrame;
                        shotgunSprite.Texture = shotgunTextures[CurrentFrame];
                    }
                }
            }

            // Вычисляем вектор направления на основе ввода
            Vector2f directionVector = new Vector2f();
            if (SPressed)
            {
                directionVector += new Vector2f(-MathF.Cos(Angle), -MathF.Sin(Angle)); // Назад
            }

            if (WPressed)
            {
                directionVector += new Vector2f(MathF.Cos(Angle), MathF.Sin(Angle)); // Вперёд
            }

            if (APressed)
            {
                directionVector += new Vector2f(MathF.Sin(Angle), -MathF.Cos(Angle)); // Влево
            }

            if (DPressed)
            {
                directionVector += new Vector2f(-MathF.Sin(Angle), MathF.Cos(Angle)); // Вправо
            }

            // Нормализуем вектор направления, чтобы длина была 1
            if (directionVector != new Vector2f(0, 0))
            {
                directionVector = directionVector /
                                  MathF.Sqrt(directionVector.X * directionVector.X +
                                             directionVector.Y * directionVector.Y);
            }

            Velocity = directionVector; // Устанавливаем скорость

            // Вычисляем смещение по X и Y
            float dx = Velocity.X * Speed * deltaTime;
            float dy = Velocity.Y * Speed * deltaTime;

            // Половина размера игрока
            float halfSizeX = Size.X / 2; // 3
            float halfSizeY = Size.Y / 2; // 3

            // Функция проверки: можно ли стоять в этой точке
            bool IsWalkable(Vector2f point)
            {
                int mapX = (int)(point.X / Tile.TILESIZE_X); // Индекс X на карте
                int mapY = (int)(point.Y / Tile.TILESIZE_Y); // Индекс Y на карте
                if (mapX < 0 || mapX >= map.Size.X || mapY < 0 || mapY >= map.Size.Y)
                    return false; // За пределами карты — нельзя
                return map.WorldMap[mapX, mapY] == 0; // Пустая клетка — можно
            }

            // Проверяем движение по оси X
            Vector2f newPosX = new Vector2f(Position.X + dx, Position.Y);
            Vector2f topLeftX = new Vector2f(newPosX.X - halfSizeX, newPosX.Y - halfSizeY);
            Vector2f topRightX = new Vector2f(newPosX.X + halfSizeX, newPosX.Y - halfSizeY);
            Vector2f bottomLeftX = new Vector2f(newPosX.X - halfSizeX, newPosX.Y + halfSizeY);
            Vector2f bottomRightX = new Vector2f(newPosX.X + halfSizeX, newPosX.Y + halfSizeY);
            bool canMoveX = IsWalkable(topLeftX) && IsWalkable(topRightX) &&
                            IsWalkable(bottomLeftX) && IsWalkable(bottomRightX);

            // Проверяем движение по оси Y
            Vector2f newPosY = new Vector2f(Position.X, Position.Y + dy);
            Vector2f topLeftY = new Vector2f(newPosY.X - halfSizeX, newPosY.Y - halfSizeY);
            Vector2f topRightY = new Vector2f(newPosY.X + halfSizeX, newPosY.Y - halfSizeY);
            Vector2f bottomLeftY = new Vector2f(newPosY.X - halfSizeX, newPosY.Y + halfSizeY);
            Vector2f bottomRightY = new Vector2f(newPosY.X + halfSizeX, newPosY.Y + halfSizeY);
            bool canMoveY = IsWalkable(topLeftY) && IsWalkable(topRightY) &&
                            IsWalkable(bottomLeftY) && IsWalkable(bottomRightY);

            // Применяем движение
            if (canMoveX && canMoveY)
            {
                // Можно двигаться по обеим осям
                Position = new Vector2f(Position.X + dx, Position.Y + dy);
            }
            else if (canMoveX)
            {
                // Можно двигаться только по X
                Position = new Vector2f(Position.X + dx, Position.Y);
            }
            else if (canMoveY)
            {
                // Можно двигаться только по Y
                Position = new Vector2f(Position.X, Position.Y + dy);
            }
            // Если движение заблокировано по обеим осям, остаёмся на месте
        }
    }
}