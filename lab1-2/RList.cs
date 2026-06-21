using System;
using System.Text;

namespace Lab12
{
    // Однозв'язний рекурсивний список цілих чисел.
    // Кожен вузол є головою свого підсписку; порожній список — це null.
    class RList
    {
        public int info;
        public RList next;

        public RList(int i)
        {
            info = i;
            next = null;
        }

        private RList(int i, RList n)
        {
            info = i;
            next = n;
        }

        // Глибока копія списку.
        public RList(RList other)
        {
            if (other == null)
                throw new ArgumentNullException("other");
            info = other.info;
            next = (other.next == null) ? null : new RList(other.next);
        }

        // Довжина списку.
        public int Length
        {
            get
            {
                if (next == null)
                    return 1;
                return 1 + next.Length;
            }
        }

        // Додати елемент на початок. Повертає нову голову.
        public RList AddFirst(int value)
        {
            return new RList(value, this);
        }

        // Додати елемент у кінець (рекурсивно).
        public void AddLastRec(int value)
        {
            if (next == null)
                next = new RList(value);
            else
                next.AddLastRec(value);
        }

        public void PrintColumn()
        {
            RList cur = this;
            while (cur != null)
            {
                Console.WriteLine(cur.info);
                cur = cur.next;
            }
        }

        // Видалити всі елементи зі значенням value. Повертає нову голову.
        public RList RemoveAll(int value)
        {
            RList head = this;
            while (head != null && head.info == value)
                head = head.next;
            if (head == null)
                return null;
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

        // Видалити елементи на парних позиціях (2, 4, 6, ...).
        public void RemoveEvenByPosition()
        {
            RList cur = this;
            while (cur.next != null)
            {
                cur.next = cur.next.next;
                if (cur.next != null)
                    cur = cur.next;
            }
        }

        // Сортування за зростанням (вибором).
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

        // Вилучити з a елементи, значення яких є в b.
        public static RList operator -(RList a, RList b)
        {
            if (a == null)
                return null;
            RList result = null;
            RList tail = null;
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

        // Конкатенація двох списків у новий.
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

        private static bool Contains(RList list, int value)
        {
            for (RList cur = list; cur != null; cur = cur.next)
                if (cur.info == value)
                    return true;
            return false;
        }

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

    class Program
    {
        static void Main()
        {
            Console.WriteLine("Демонстрація класу RList\n");

            RList list = new RList(5);
            Console.WriteLine("Список з 1 елемента: " + list);

            list = list.AddFirst(3);
            list = list.AddFirst(1);
            Console.WriteLine("Після AddFirst(3), AddFirst(1): " + list);

            list.AddLastRec(8);
            list.AddLastRec(4);
            Console.WriteLine("Після AddLastRec(8), AddLastRec(4): " + list);

            Console.WriteLine("Довжина: " + list.Length);

            Console.WriteLine("\nДрук у стовпець:");
            list.PrintColumn();

            RList copy = new RList(list);
            copy.AddLastRec(99);
            Console.WriteLine("\nКопія після AddLastRec(99): " + copy);
            Console.WriteLine("Оригінал: " + list);

            list.SortAsc();
            Console.WriteLine("\nПісля SortAsc(): " + list);

            RList list2 = new RList(10);
            list2.AddLastRec(20); list2.AddLastRec(30);
            list2.AddLastRec(40); list2.AddLastRec(50);
            Console.WriteLine("\nСписок: " + list2);
            list2.RemoveEvenByPosition();
            Console.WriteLine("Після RemoveEvenByPosition(): " + list2);

            RList list3 = new RList(7);
            list3.AddLastRec(2); list3.AddLastRec(7); list3.AddLastRec(7);
            list3.AddLastRec(5); list3.AddLastRec(7);
            Console.WriteLine("\nСписок: " + list3);
            list3 = list3.RemoveAll(7);
            Console.WriteLine("Після RemoveAll(7): " + list3);

            RList a = new RList(1);
            a.AddLastRec(2); a.AddLastRec(3); a.AddLastRec(4); a.AddLastRec(5);
            RList b = new RList(2); b.AddLastRec(4);
            RList diff = a - b;
            Console.WriteLine("\na = " + a);
            Console.WriteLine("b = " + b);
            Console.WriteLine("a - b: " + diff);

            RList sum = a + b;
            Console.WriteLine("a + b: " + sum);
            Console.WriteLine("a: " + a);
        }
    }
}
