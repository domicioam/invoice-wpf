namespace NFe.Core.Utils.Assinatura
{
    public class Certificate
    {
        public string FriendlySubjectName { get; set; }
        public string FriendlyIssuer { get; set; }
        public string SerialNumber { get; set; }

        public override string ToString()
        {
            return FriendlySubjectName + " - " + FriendlyIssuer;
        }
    }
}
