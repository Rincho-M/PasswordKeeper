namespace PasswordKeeper.Core.Utility
{
    class PasswordType
    {
        public bool IsNumberRequired { get; set; }
        public bool IsUpperRequired { get; set; }
        public bool IsSymbolRequired { get; set; }

        public PasswordType(bool isNumberRequired, bool isUppderRequired, bool isSymbolRequired)
        {
            IsNumberRequired = isNumberRequired;
            IsUpperRequired = isUppderRequired;
            IsSymbolRequired = isSymbolRequired;
        }
    }
}
