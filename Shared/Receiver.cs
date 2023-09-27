

namespace Shared
{
    public class Receiver : ValueObject

    {
        public string Name { get; set; }
        public string ID_NO { get; set; }
        public string Phone_No { get; set; }
        public string Dst_Account { get; set; }

        public Receiver() { }

        public Receiver(string rec_Name, string rec_ID_NO, string rec_Phone, string rec_Dst_Acc)
        {
            Name = rec_Name;
            ID_NO = rec_ID_NO;
            Phone_No = rec_Phone;
            Dst_Account = rec_Dst_Acc;
        }


        protected override IEnumerable<object> GetEqualityComponents()
        {
            // Using a yield return statement to return each element one at a time
            yield return Name;
            yield return ID_NO;
            yield return Phone_No;
            yield return Dst_Account;

        }
    }
}
