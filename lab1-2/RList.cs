using System;
using System.Text;

namespace Lab12
{
    // Рекурсивний однозв'язний список цілих чисел.
    // Кожен екземпляр класу — це вузол, який водночас є головою свого підсписку.
    // Порожній список представляється посиланням null.
    class RList
    {
        // Значення, що зберігається у вузлі.
        public int info;
        // Посилання на наступний вузол (null — кінець списку).
        public RList next;

        // (Елемент 1) Конструктор з одним параметром: список з одного елемента.
        public RList(int i)
        {
            info = i;
            next = null;
        }

        // Допоміжний приватний конструктор (значення + наступний вузол)
        // для зручної внутрішньої побудови списку.
        private RList(int i, RList n)
        {
            info = i;
            next = n;
        }

        // (Елемент 3) Конструктор копіювання: глибока копія всього списку.
        public RList(RList other)
        {
            if (other == null)
                throw new ArgumentNullException("other", "Не можна копіювати порожнє посилання.");
            info = other.info;
            // Рекурсивне копіювання робить копію повністю незалежною від оригіналу.
            next = (other.next == null) ? null : new RList(other.next);
        }

        // (Елемент 49) Властивість Length (лише читання): довжина списку.
        // Реалізована рекурсивно у стилі лекційного прикладу.
        public int Length
        {
            get
            {
                if (next == null)
                    return 1;
                return 1 + next.Length;
            }
        }

        // (Елемент 4) Додавання нового елемента першим.
        // Голова списку змінюється, тому метод повертає нову голову.
        // Виклик: list = list.AddFirst(value);
        public RList AddFirst(int value)
        {
            return new RList(value, this);
        }

        // (Елемент 6) Рекурсивне додавання нового елемента останнім.
        // Голова не змінюється, тому метод нічого не повертає.
        public void AddLastRec(int value)
        {
            if (next == null)
                next = new RList(value);
            else
                next.AddLastRec(value);
        }

        // (Елемент 27) Нерекурсивний друк елементів у прямому порядку у стовпець.
        public void PrintColumn()
        {
            RList cur = this;
            while (cur != null)
            {
                Console.WriteLine(cur.info);
                cur = cur.next;
            }
        }

        // (Елемент 19) Нерекурсивне видалення всіх елементів із заданим значенням.
        // Голова може бути видалена, тому повертаємо нову голову (можливо null).
        // Виклик: list = list.RemoveAll(value);
        public RList RemoveAll(int value)
        {
            // Пропускаємо збіги на початку, щоб знайти нову голову.
            RList head = this;
            while (head != null && head.info == value)
                head = head.next;

            if (head == null)
                return null; // усі елементи мали задане значення

            // Видаляємо збіги всередині/в кінці.
            RList cur = head;
            while (cur.next != null)
            {
                if (cur.next.info == value)
                    cur.next = cur.next.next;
                else
                    cur = cur.next;
            }
            return head;
        }

        // (Елемент 23) Видалення всіх парних за порядком елементів (позиції 2, 4, 6, ...).
        // Перша позиція завжди залишається, тому голова не змінюється.
        public void RemoveEvenByPosition()
        {
            RList cur = this; // завжди вказує на елемент непарної позиції
            while (cur.next != null)
            {
                cur.next = cur.next.next; // видаляємо елемент парної позиції
                if (cur.next != null)
                    cur = cur.next;       // переходимо до наступного непарного
            }
        }

        // (Елемент 39) Сортування за зростанням значень (сортування вибором).
        // Переставляються лише значення; довжина і структура не змінюються.
        public void SortAsc()
        {
            for (RList i = this; i != null; i = i.next)
            {
                RList min = i;
                for (RList j = i.next; j != null; j = j.next)
                    if (j.info < min.info)
                        min = j;
                if (min != i)
                {
                    int tmp = i.info;
                    i.info = min.info;
                    min.info = tmp;
                }
            }
        }

        // (Елемент 64) Операція "-": з лівого списку вилучаються всі елементи,
        // значення яких присутні у правому списку. Повертає новий список (можливо null).
        public static RList operator -(RList a, RList b)
        {
            if (a == null)
                return null;
            RList result = null; // голова нового списку
            RList tail = null;   // хвіст для додавання в кінець
            for (RList cur = a; cur != null; cur = cur.next)
            {
                if (!Contains(b, cur.info))
                {
                    RList node = new RList(cur.info);
                    if (result == null)
                        result = tail = node;
                    else
                    {
                        tail.next = node;
                        tail = node;
                    }
                }
            }
            return result;
        }

        // (Елемент 79) Операція "+": конкатенація двох списків у новий
        // (глибокі копії a, потім b). Оригінали не змінюються.
        public static RList operator +(RList a, RList b)
        {
            RList copyA = (a == null) ? null : new RList(a);
            RList copyB = (b == null) ? null : new RList(b);
            if (copyA == null)
                return copyB;
            RList cur = copyA;
            while (cur.next != null)
                cur = cur.next;
            cur.next = copyB;
            return copyA;
        }

        // Допоміжний метод: чи містить список задане значення (для операції "-").
        private static bool Contains(RList list, int value)
        {
            for (RList cur = list; cur != null; cur = cur.next)
                if (cur.info == value)
                    return true;
            return false;
        }

        // Зручне рядкове подання списку, напр. "1 -> 3 -> 5".
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            RList cur = this;
            while (cur != null)
            {
                sb.Append(cur.info);
                if (cur.next != null)
                    sb.Append(" -> ");
                cur = cur.next;
            }
            return sb.ToString();
        }
    }

    // Клас з точкою входу для тестування класу RList.
    class Program
    {
        static void Main()
        {
            Console.WriteLine("=== Демонстрація класу RList (варіант 2) ===\n");

            // (Елемент 1) Конструктор з одним параметром.
            RList list = new RList(5);
            Console.WriteLine("Список з 1 елемента (конструктор з 1 параметром): " + list);

            // (Елемент 4) Додавання першим.
            list = list.AddFirst(3);
            list = list.AddFirst(1);
            Console.WriteLine("Після AddFirst(3), AddFirst(1):                   " + list);

            // (Елемент 6) Рекурсивне додавання останнім.
            list.AddLastRec(8);
            list.AddLastRec(4);
            Console.WriteLine("Після AddLastRec(8), AddLastRec(4):               " + list);

            // (Елемент 49) Властивість Length.
            Console.WriteLine("Довжина (властивість Length): " + list.Length);

            // (Елемент 27) Нерекурсивний друк у стовпець.
            Console.WriteLine("\nДрук у стовпець (елемент 27):");
            list.PrintColumn();

            // (Елемент 3) Конструктор копіювання + перевірка незалежності копії.
            RList copy = new RList(list);
            copy.AddLastRec(99); // змінюємо лише копію
            Console.WriteLine("\nКопія після AddLastRec(99): " + copy);
            Console.WriteLine("Оригінал (не змінився):     " + list);

            // (Елемент 39) Сортування за зростанням.
            list.SortAsc();
            Console.WriteLine("\nПісля SortAsc(): " + list);

            // (Елемент 23) Видалення парних за позицією.
            RList list2 = new RList(10);
            list2.AddLastRec(20); list2.AddLastRec(30);
            list2.AddLastRec(40); list2.AddLastRec(50);
            Console.WriteLine("\nСписок для видалення за позицією: " + list2);
            list2.RemoveEvenByPosition();
            Console.WriteLine("Після RemoveEvenByPosition() (видалено поз. 2,4): " + list2);

            // (Елемент 19) Видалення всіх елементів із заданим значенням.
            RList list3 = new RList(7);
            list3.AddLastRec(2); list3.AddLastRec(7); list3.AddLastRec(7);
            list3.AddLastRec(5); list3.AddLastRec(7);
            Console.WriteLine("\nСписок для видалення значення: " + list3);
            list3 = list3.RemoveAll(7);
            Console.WriteLine("Після RemoveAll(7): " + list3);

            // (Елемент 64) Операція "-".
            RList a = new RList(1);
            a.AddLastRec(2); a.AddLastRec(3); a.AddLastRec(4); a.AddLastRec(5);
            RList b = new RList(2); b.AddLastRec(4);
            RList diff = a - b;
            Console.WriteLine("\na = " + a);
            Console.WriteLine("b = " + b);
            Console.WriteLine("a - b (елемент 64): " + diff);

            // (Елемент 79) Операція "+".
            RList sum = a + b;
            Console.WriteLine("a + b (елемент 79): " + sum);
            Console.WriteLine("a (не змінився):    " + a);
        }
    }
}
