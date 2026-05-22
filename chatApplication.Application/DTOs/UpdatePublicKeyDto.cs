using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace chatApplication.Application.DTOs
{
    public class UpdatePublicKeyDto
    {
        [Required(ErrorMessage = "Public key gerekli")]
        public string PublicKey { get; set; } = string.Empty;
    }
}
