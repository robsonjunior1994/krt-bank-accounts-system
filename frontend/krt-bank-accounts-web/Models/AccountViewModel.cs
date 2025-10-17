using System.ComponentModel.DataAnnotations;

namespace krt_bank_accounts_web.Models
{
    public class AccountViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "O nome é obrigatório.")]
        [Display(Name = "Nome do Titular")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "O CPF é obrigatório.")]
        [Display(Name = "CPF")]
        [RegularExpression(@"^\d{11}$", ErrorMessage = "Informe um CPF válido com 11 dígitos.")]
        public string Cpf { get; set; } = string.Empty;

        public string Status { get; set; } = "Active";
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
