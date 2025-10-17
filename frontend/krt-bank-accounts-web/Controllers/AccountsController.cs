using krt_bank_accounts_web.Models;
using krt_bank_accounts_web.Services;
using Microsoft.AspNetCore.Mvc;

namespace krt_bank_accounts_web.Controllers
{
    public class AccountsController : Controller
    {
        private readonly AccountApiService _service;

        public AccountsController(AccountApiService service)
        {
            _service = service;
        }

        public async Task<IActionResult> Index(int pageNumber = 1, int pageSize = 5)
        {
            var pagedData = await _service.GetAllAsync(pageNumber, pageSize);

            if (pagedData == null)
                return View(new PagedResponse<AccountViewModel>());

            return View(pagedData);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AccountViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            await _service.CreateAsync(model);
            TempData["SuccessMessage"] = "Conta criada com sucesso!";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var conta = await _service.GetByIdAsync(id);

            if (conta == null)
                return NotFound();

            return View(conta);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, AccountViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            await _service.UpdateAsync(id, model);
            TempData["SuccessMessage"] = "Conta atualizada com sucesso!";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var conta = await _service.GetByIdAsync(id);

            if (conta == null)
                return NotFound();

            return View(conta);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id, AccountViewModel model)
        {
            await _service.DeleteAsync(id, model);
            TempData["SuccessMessage"] = "Conta excluída com sucesso!";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> ToggleStatus(int id, bool activate)
        {
            var result = await _service.UpdateStatusAsync(id, activate);

            if (result != null && result.IsSuccess)
                TempData["SuccessMessage"] = "Status da conta atualizado com sucesso!";
            else
                TempData["ErrorMessage"] = "Não foi possível atualizar o status.";

            return RedirectToAction(nameof(Index));
        }


    }
}
