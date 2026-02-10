namespace WebApplication2.Models
{
    public class Coil
    {
        public Coil() { }
        public Coil(double length, double weigth, DateTime dateAdd)
        {
            Length = length;
            Weight = weigth;
            DateAdd = dateAdd;
        }
        public Coil(int id, double length, double weigth, DateTime dateAdd, DateTime dateDelete)
        {
            Id = id;
            Length = length;
            Weight = weigth;
            DateAdd = dateAdd;
            DateDelete = dateDelete;
        }
        public int Id { get; set; }
        private double length;
        public double Length
        {
            get => length;
            set
            {
                if (value <= 0)
                    throw new ArgumentException("не допустимое значение длины");
                else length = value;
            }
        }
        private double weigth;
        public double Weight
        {
            get => weigth;
            set
            {
                if (value < 0)
                    throw new ArgumentException("не допустимое значение массы");
                else weigth = value;
            }
        }
        private DateTime? dateAdd;
        public DateTime? DateAdd
        {
            get => dateAdd;
            set
            {
                if (DateDelete != null && value > DateDelete)
                    throw new ArgumentException("дата добавления не может быть позже даты удаления");
                else dateAdd = value;
            }
        }
        private DateTime? dateDelete;
        public DateTime? DateDelete
        {
            get => dateDelete;
            set
            {
                if (DateAdd != null && value < DateAdd)
                    throw new ArgumentException("дата удаления не может быть раньше даты добавления");
                else dateDelete = value;
            }
        }
    }
}
