using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace PokemonROMEditor.Models
{
    public class ByteRangeRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            int byteValue = 0;

            try
            {
                if (((string)value).Length > 0)
                    byteValue = Int32.Parse((String)value);
            }
            catch (Exception e)
            {
                return new ValidationResult(false, "Illegal characters or " + e.Message);
            }

            if ((byteValue < 1) || (byteValue > 255))
            {
                return new ValidationResult(false,
                  "Please enter a value in the range: " + 1 + " - " + 255 + ".");
            }
            else
            {
                return ValidationResult.ValidResult;
            }
        }
    }
}
