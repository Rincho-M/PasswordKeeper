namespace PasswordKeeper.Core.Models
{
    class PasswordTypeModel
    {
        public bool IsNumberRequired { get; set; }
        public bool IsUpperRequired { get; set; }
        public bool IsSymbolRequired { get; set; }

        public PasswordTypeModel(bool isNumberRequired, bool isUppderRequired, bool isSymbolRequired)
        {
            IsNumberRequired = isNumberRequired;
            IsUpperRequired = isUppderRequired;
            IsSymbolRequired = isSymbolRequired;
        }
    }
}
