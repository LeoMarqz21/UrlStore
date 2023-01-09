using System.ComponentModel.DataAnnotations;

namespace UrlStore.Validations
{
    public class ValidFileTypes : ValidationAttribute
    {
        private readonly string[] validFileTypes;

        public ValidFileTypes(string[] validFileTypes)
        {
            this.validFileTypes = validFileTypes;
        }

        public ValidFileTypes(FileType fileType)
        {
            switch (fileType)
            {
                case FileType.Image:
                    validFileTypes = new string[] { "image/jpeg", "image/png", "image/gif", "image/jpg" };
                    break;
                case FileType.Pdf:
                    break;
                case FileType.Txt:
                    break;
                default:
                    break;
            }
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value is null) return ValidationResult.Success;
            IFormFile file = value as IFormFile;
            if (file is null) return ValidationResult.Success;
            if (!validFileTypes.Contains(file.ContentType))
            {
                return new ValidationResult($"Valid file types: {string.Join(", ", validFileTypes)}");
            }
            return ValidationResult.Success;
        }
    }
}
