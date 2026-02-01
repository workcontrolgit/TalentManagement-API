// Defines an asynchronous repository interface for the Department entity
namespace TalentManagementAPI.Application.Interfaces.Repositories
{
    public interface IDepartmentRepositoryAsync : IGenericRepositoryAsync<Department>
    {
        Task<(IEnumerable<Entity> data, RecordsCount recordsCount)> GetDepartmentResponseAsync(GetDepartmentsQuery requestParameters);
    }
}

