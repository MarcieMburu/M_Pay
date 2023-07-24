namespace PaymentGateway.Models
{
    public class Sender : ValueObject
    {
        public string Name { get; set; }
        public string ID_NO { get; set; }
        public string Phone_No { get; set; }
        public string Src_Account { get; set; }

        public Sender() { }

        public Sender(string sen_Name, string sen_ID_NO, string sen_Phone, string sen_Src_Acc)
        {
            Name = sen_Name;
            ID_NO = sen_ID_NO;
            Phone_No = sen_Phone;
            Src_Account = sen_Src_Acc;
        }


        protected override IEnumerable<object> GetEqualityComponents()
        {
            // Using a yield return statement to return each element one at a time
            yield return Name;
            yield return ID_NO;
            yield return Phone_No;
            yield return Src_Account;

        }
    }
}
