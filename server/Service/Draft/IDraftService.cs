using Service.Draft.Dto;

namespace Service.Draft;

public interface IDraftService
{
    DraftDetail GetById(long id);
    IEnumerable<Dto.Draft> List();
    Task<long> Create(DraftFormData data);
    Task Update(long id, DraftFormData data);
    Task Delete(long id);
}