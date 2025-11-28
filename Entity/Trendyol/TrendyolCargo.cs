namespace Pazaryeri.Entity.Trendyol
{
    public class TrendyolCargo
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public long TaxNumber { get; set; }

        public static List<TrendyolCargo> CargoList = new List<TrendyolCargo>
        {
            new TrendyolCargo { Id = 38, Code = "SENDEOMP", Name = "Kolay Gelsin Marketplace",      TaxNumber = 2910804196 },
            new TrendyolCargo { Id = 30, Code = "BORMP",    Name = "Borusan Lojistik Marketplace",   TaxNumber = 1800038254 },
            new TrendyolCargo { Id = 10, Code = "DHLECOMMP",Name = "DHL eCommerce Marketplace",      TaxNumber = 6080712084 },
            new TrendyolCargo { Id = 19, Code = "PTTMP",    Name = "PTT Kargo Marketplace",          TaxNumber = 7320068060 },
            new TrendyolCargo { Id = 9,  Code = "SURATMP",  Name = "Sürat Kargo Marketplace",        TaxNumber = 7870233582 },
            new TrendyolCargo { Id = 17, Code = "TEXMP",    Name = "Trendyol Express Marketplace",   TaxNumber = 8590921777 },
            new TrendyolCargo { Id = 6,  Code = "HOROZMP",  Name = "Horoz Kargo Marketplace",        TaxNumber = 4630097122 },
            new TrendyolCargo { Id = 20, Code = "CEVAMP",   Name = "CEVA Marketplace",               TaxNumber = 8450298557 },
            new TrendyolCargo { Id = 4,  Code = "YKMP",     Name = "Yurtiçi Kargo Marketplace",      TaxNumber = 3130557669 },
            new TrendyolCargo { Id = 7,  Code = "ARASMP",   Name = "Aras Kargo Marketplace",         TaxNumber = 720039666  }
        };
    }
}
