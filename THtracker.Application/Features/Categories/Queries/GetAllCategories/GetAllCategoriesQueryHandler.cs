using MediatR;
using THtracker.Domain.Interfaces;

namespace THtracker.Application.Features.Categories.Queries.GetAllCategories;

public sealed class GetAllCategoriesQueryHandler : IRequestHandler<GetAllCategoriesQuery, IEnumerable<CategoryResponse>>
{
    private readonly ICategoryRepository _categoryRepository;

    public GetAllCategoriesQueryHandler(ICategoryRepository categoryRepository)
    {
        _categoryRepository = categoryRepository;
    }

    public async Task<IEnumerable<CategoryResponse>> Handle(GetAllCategoriesQuery request, CancellationToken cancellationToken)
    {
        var categories = await _categoryRepository.GetAllByUserAsync(request.UserId, cancellationToken);
        
        return categories
            .Select(c => new CategoryResponse(
                c.Id, 
                c.UserId, 
                c.Name, 
                c.Description, 
                c.Color));
    }
}
