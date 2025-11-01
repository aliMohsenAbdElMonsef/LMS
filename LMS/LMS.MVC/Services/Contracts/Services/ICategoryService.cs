using LMS.MVC.Models.ViewModels.Category;

namespace LMS.MVC.Services.Contracts.Services
{
    public interface ICategoryService: IBaseMVCServices
    {
        Task<IEnumerable<ReadCategoryResult>> GetAllCategories();
        Task<CategoryDetailsResult> GetCategoryById(string Id);

        Task<ReadCategoryResult> GetEditModel(string Id);
        ReadCategoryResult GetCreateModel();
        Task<ReadCategoryResult> EditCategory(ReadCategoryResult model);
        Task<ReadCategoryResult> CreateCategory(ReadCategoryResult model);
    }
}
