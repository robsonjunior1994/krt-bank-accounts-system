using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace KRT.BankAccounts.Api._01_Presentation.DTOs.Request;

[ExcludeFromCodeCoverage]
public class CreateAccountRequest
{
    [Required(ErrorMessage = "O nome é obrigatório.")]
    [StringLength(100, MinimumLength = 3, ErrorMessage = "O nome deve ter entre 3 e 100 caracteres.")]
    public string Name { get; set; }

    [Required(ErrorMessage = "O CPF é obrigatório.")]
    [RegularExpression(@"^\d{11}$", ErrorMessage = "O CPF deve conter exatamente 11 dígitos numéricos.")]
    public string Cpf { get; set; }
}
