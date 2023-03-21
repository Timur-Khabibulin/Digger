using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Forms;
using System.Runtime.CompilerServices;

namespace Digger
{
    public class Terrain : ICreature
    {
        CreatureCommand ICreature.Act(int x, int y) => new CreatureCommand();

        bool ICreature.DeadInConflict(ICreature conflictedObject)
            => conflictedObject is Digger.Player;

        int ICreature.GetDrawingPriority() => 0;

        /// <returns>Название файла с изображением земли</returns>
        string ICreature.GetImageFileName() => "Terrain.png";
    }

    public class Player : ICreature
    {
        public static int X = 0; //Координата x игрока
        public static int Y = 0; //Координата y игрока

        /// <summary>
        /// Метод реализует движение диггера
        /// </summary>
        /// <param name="x">Координата x</param>
        /// <param name="y">Координата y</param>
        /// <returns>Объект класса CreatureCommand</returns>
        CreatureCommand ICreature.Act(int x, int y)
        {
            var (dx, dy) = GetDeltaPos(x, y); //Получаем смещение игрока

            //Если в новой клетке золото
            if (Game.Map[x + dx, y + dy] is Digger.Gold)
            {
                Game.Scores += 10; //Увеличиваем очки
                Game.Map[x + dx, y + dy].DeadInConflict(this); //Убираем золото
            }
            //Если в новой клетке мешок с золотом, прийти в неё нельзя
            else if (Game.Map[x + dx, y + dy] is Digger.Sack)
                return new CreatureCommand { DeltaX = 0, DeltaY = 0 };

            return new CreatureCommand { DeltaX = dx, DeltaY = dy };
        }

        /// <param name="conflictedObject">Объект, находящийся в этой же клетке</param>
        /// <returns>Наличие факта смерти</returns>
        bool ICreature.DeadInConflict(ICreature conflictedObject)
            => conflictedObject is Digger.Sack || conflictedObject is Digger.Monster;

        /// <returns>Приоритет отрисовки</returns>
        int ICreature.GetDrawingPriority() => 0;

        /// <returns>Название файла с изображением игрока</returns>
        string ICreature.GetImageFileName() => "Digger.png";

        /// <summary>
        /// Метод реализует управление диггером
        /// </summary>
        /// <param name="x">Координата x</param>
        /// <param name="y">Координата y</param>
        /// <returns>Кортеж из смещений по Ox и Oy</returns>
        private (int, int) GetDeltaPos(int x, int y)
        {
            var dx = 0; //Сдвиг по Ox
            var dy = 0; //Сдвиг по Oy
            switch (Game.KeyPressed)
            {
                //Если не вышли за пределы поля двигаемся
                case Keys.Up: //Вверх
                    if (y > 0) dy--;
                    break;
                case Keys.Down: //Вниз
                    if (y < Game.MapHeight - 1) dy++;
                    break;
                case Keys.Right: //Вправо
                    if (x < Game.MapWidth - 1) dx++;
                    break;
                case Keys.Left: //Влево
                    if (x > 0) dx--;
                    break;
            }
            return (dx, dy);
        }
    }

    public class Sack : ICreature
    {
        private int numOfMovedCells = 0; //количество клеток на которые сдвинулся мешок

        /// <summary>
        /// Реализация движений мешка с золотом
        /// </summary>
        /// <param name="x">Координата x</param>
        /// <param name="y">Координата y</param>
        /// <returns>Объект класса CreatureCommand</returns>
        public CreatureCommand Act(int x, int y)
        {
            //Если не вышли за поле
            if (y + 1 < Game.MapHeight)
            {
                //Если следующая клетка пустая
                if (Game.Map[x, y + 1] == null)
                {
                    numOfMovedCells++;
                    return new CreatureCommand { DeltaX = 0, DeltaY = 1 };
                }
                //Если мешок уже падал и в следующей клетке игрок либо монстр
                else if (numOfMovedCells > 0 && (Game.Map[x, y + 1] is Digger.Player
                            || Game.Map[x, y + 1] is Digger.Monster))
                {
                    numOfMovedCells++;
                    Game.Map[x, y + 1].DeadInConflict(this); //Убиваем игрока/монста
                    return new CreatureCommand { DeltaX = 0, DeltaY = 1 };
                }
                //Если мешок не может дальше падать
                else if (numOfMovedCells == 1) numOfMovedCells = 0;
            }
            //Если мешок падал дольше 1 клетки, превращаем его в золото
            return numOfMovedCells > 1 ? new CreatureCommand { DeltaX = 0, DeltaY = 0, TransformTo = new Gold() }
                             : new CreatureCommand { DeltaX = 0, DeltaY = 0 };
        }

        /// <param name="conflictedObject">Объект, находящийся в этой же клетке</param>
        /// <returns>Наличие факта смерти</returns>
        bool ICreature.DeadInConflict(ICreature conflictedObject) => conflictedObject == null;

        /// <returns>Приоритет отрисовки</returns>
        int ICreature.GetDrawingPriority() => 0;

        /// <returns>Название файла с изображением мешка с золотом</returns>
        string ICreature.GetImageFileName() => "Sack.png";
    }

    class Gold : ICreature
    {
        CreatureCommand ICreature.Act(int x, int y) => new CreatureCommand();

        /// <param name="conflictedObject">Объект, находящийся в этой же клетке</param>
        /// <returns>Наличие факта смерти</returns>
        bool ICreature.DeadInConflict(ICreature conflictedObject) => conflictedObject != null;

        /// <returns>Приоритет отрисовки</returns>
        int ICreature.GetDrawingPriority() => 0;

        /// <returns>Название файла с изображением золота</returns>
        string ICreature.GetImageFileName() => "Gold.png";
    }

    class Monster : ICreature
    {
        /// <summary>
        /// Реализация движений монстра
        /// </summary>
        /// <param name="x">Координата x</param>
        /// <param name="y">Координата y</param>
        /// <returns>Объект класса CreatureCommand</returns>
        CreatureCommand ICreature.Act(int x, int y)
        {
            var dx = 0; //Смещение по Ox
            var dy = 0; //Смещение по Oy

            if (FindPlayer()) //Если на карте есть игрок
            {
                //Если у игрока и монстра одна координата x
                if (Player.X == x) dy = Player.Y > y && Player.Y != y ? 1 : -1;
                //Если у игрока и монстра одна координата y
                else if (Player.Y == y) dx = Player.X > x && Player.X != x ? 1 : -1;
                //Если они не имеют равных координат
                else dx = Player.X > x && Player.X != x ? 1 : -1;
                //Объект, находящийся в следующей клетке
                var obj = Game.Map[x + dx, y + dy];
                //Если не вышли за край поля
                if (x + dx >= 0 && x + dx < Game.MapWidth && y + dy >= 0 && y + dy < Game.MapHeight)
                {
                    //Если объект является землей, мешком или монстром, не двигаемся
                    if (obj is Digger.Terrain || obj is Digger.Sack || obj is Digger.Monster)
                        return new CreatureCommand() { DeltaX = 0, DeltaY = 0 };
                }
            }
            return new CreatureCommand() { DeltaX = dx, DeltaY = dy };
        }

        /// <summary>
        /// Метод находит координаты игрока
        /// </summary>
        /// <returns>true - игрок найден, иначе - false</returns>
        private bool FindPlayer()
        {
            for (int i = 0; i < Game.MapWidth; i++)
            {
                for (int j = 0; j < Game.MapHeight; j++)
                {
                    if (Game.Map[i, j] is Digger.Player)
                    {
                        Player.X = i;
                        Player.Y = j;
                        return true;
                    }
                }
            }
            return false;
        }

        /// <param name="conflictedObject">Объект, находящийся в этой же клетке</param>
        /// <returns>Наличие факта смерти</returns>
        bool ICreature.DeadInConflict(ICreature conflictedObject)
           => conflictedObject is Digger.Sack || conflictedObject is Digger.Monster;

        /// <returns>Приоритет отрисовки</returns>
        int ICreature.GetDrawingPriority() => 0;

        /// <returns>Название файла с изображением монстрра</returns>
        string ICreature.GetImageFileName() => "Monster.png";
    }
}