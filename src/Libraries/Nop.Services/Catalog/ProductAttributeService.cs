using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Media;
using Nop.Data;

namespace Nop.Services.Catalog;

/// <summary>
/// Product attribute service
/// </summary>
public partial class ProductAttributeService : IProductAttributeService
{
    #region Fields

    protected readonly IRepository<Picture> _pictureRepository;
    protected readonly IRepository<PredefinedProductAttributeValue> _predefinedProductAttributeValueRepository;
    protected readonly IRepository<Product> _productRepository;
    protected readonly IRepository<ProductAttribute> _productAttributeRepository;
    protected readonly IRepository<ProductAttributeCombination> _productAttributeCombinationRepository;
    protected readonly IRepository<ProductAttributeCombinationPicture> _productAttributeCombinationPictureRepository;
    protected readonly IRepository<ProductAttributeMapping> _productAttributeMappingRepository;
    protected readonly IRepository<ProductAttributeValue> _productAttributeValueRepository;
    protected readonly IRepository<ProductAttributeValuePicture> _productAttributeValuePictureRepository;
    protected readonly IRepository<ProductPicture> _productPictureRepository;
    protected readonly IStaticCacheManager _staticCacheManager;

    #endregion

    #region Ctor

    public ProductAttributeService(IRepository<Picture> pictureRepository,
        IRepository<PredefinedProductAttributeValue> predefinedProductAttributeValueRepository,
        IRepository<Product> productRepository,
        IRepository<ProductAttribute> productAttributeRepository,
        IRepository<ProductAttributeCombination> productAttributeCombinationRepository,
        IRepository<ProductAttributeCombinationPicture> productAttributeCombinationPictureRepository,
        IRepository<ProductAttributeMapping> productAttributeMappingRepository,
        IRepository<ProductAttributeValue> productAttributeValueRepository,
        IRepository<ProductAttributeValuePicture> productAttributeValuePictureRepository,
        IRepository<ProductPicture> productPictureRepository,
        IStaticCacheManager staticCacheManager)
    {
        _pictureRepository = pictureRepository;
        _predefinedProductAttributeValueRepository = predefinedProductAttributeValueRepository;
        _productRepository = productRepository;
        _productAttributeRepository = productAttributeRepository;
        _productAttributeCombinationRepository = productAttributeCombinationRepository;
        _productAttributeCombinationPictureRepository = productAttributeCombinationPictureRepository;
        _productAttributeMappingRepository = productAttributeMappingRepository;
        _productAttributeValueRepository = productAttributeValueRepository;
        _productAttributeValuePictureRepository = productAttributeValuePictureRepository;
        _productPictureRepository = productPictureRepository;
        _staticCacheManager = staticCacheManager;
    }

    #endregion

    #region Methods

    #region Product attributes

    /// <inheritdoc />
    public virtual async Task DeleteProductAttributeAsync(ProductAttribute productAttribute)
    {
        await _productAttributeRepository.DeleteAsync(productAttribute);
    }

    /// <inheritdoc />
    public virtual async Task DeleteProductAttributesAsync(IList<ProductAttribute> productAttributes)
    {
        ArgumentNullException.ThrowIfNull(productAttributes);

        await _productAttributeRepository.DeleteAsync(productAttributes);
    }

    /// <inheritdoc />
    public virtual async Task<IPagedList<ProductAttribute>> GetAllProductAttributesAsync(string name = null, int pageIndex = 0, int pageSize = int.MaxValue)
    {
        var query = _productAttributeRepository.Table;

        if (!string.IsNullOrWhiteSpace(name))
            query = query.Where(pa => pa.Name.Contains(name));

        return await query.OrderBy(x => x.Name).ToPagedListAsync(pageIndex, pageSize);
    }

    /// <inheritdoc />
    public virtual async Task<ProductAttribute> GetProductAttributeByIdAsync(int productAttributeId)
    {
        return await _productAttributeRepository.GetByIdAsync(productAttributeId, cache => default);
    }

    /// <inheritdoc />
    public virtual async Task<IList<ProductAttribute>> GetProductAttributeByIdsAsync(int[] productAttributeIds)
    {
        return await _productAttributeRepository.GetByIdsAsync(productAttributeIds);
    }

    /// <inheritdoc />
    public virtual async Task InsertProductAttributeAsync(ProductAttribute productAttribute)
    {
        await _productAttributeRepository.InsertAsync(productAttribute);
    }

    /// <inheritdoc />
    public virtual async Task UpdateProductAttributeAsync(ProductAttribute productAttribute)
    {
        await _productAttributeRepository.UpdateAsync(productAttribute);
    }

    /// <inheritdoc />
    public virtual async Task<int[]> GetNotExistingAttributesAsync(int[] attributeId)
    {
        ArgumentNullException.ThrowIfNull(attributeId);

        var query = _productAttributeRepository.Table;
        var queryFilter = attributeId.Distinct().ToArray();
        var filter = await query.Select(a => a.Id)
            .Where(m => queryFilter.Contains(m))
            .ToListAsync();

        return queryFilter.Except(filter).ToArray();
    }

    #endregion

    #region Product attributes mappings

    /// <inheritdoc />
    public virtual async Task DeleteProductAttributeMappingAsync(ProductAttributeMapping productAttributeMapping)
    {
        await _productAttributeMappingRepository.DeleteAsync(productAttributeMapping);
    }

    /// <inheritdoc />
    public virtual async Task<IList<ProductAttributeMapping>> GetProductAttributeMappingsByProductIdAsync(int productId)
    {
        var allCacheKey = _staticCacheManager.PrepareKeyForDefaultCache(NopCatalogDefaults.ProductAttributeMappingsByProductCacheKey, productId);

        var query = from pam in _productAttributeMappingRepository.Table
            orderby pam.DisplayOrder, pam.Id
            where pam.ProductId == productId
            select pam;

        var attributes = await _staticCacheManager.GetAsync(allCacheKey, async () => await query.ToListAsync()) ?? new List<ProductAttributeMapping>();

        return attributes;
    }

    /// <inheritdoc />
    public virtual async Task<ProductAttributeMapping> GetProductAttributeMappingByIdAsync(int productAttributeMappingId)
    {
        return await _productAttributeMappingRepository.GetByIdAsync(productAttributeMappingId, cache => default);
    }

    /// <inheritdoc />
    public virtual async Task InsertProductAttributeMappingAsync(ProductAttributeMapping productAttributeMapping)
    {
        await _productAttributeMappingRepository.InsertAsync(productAttributeMapping);
    }

    /// <inheritdoc />
    public virtual async Task UpdateProductAttributeMappingAsync(ProductAttributeMapping productAttributeMapping)
    {
        await _productAttributeMappingRepository.UpdateAsync(productAttributeMapping);
    }

    #endregion

    #region Product attribute values

    /// <inheritdoc />
    public virtual async Task DeleteProductAttributeValueAsync(ProductAttributeValue productAttributeValue)
    {
        await _productAttributeValueRepository.DeleteAsync(productAttributeValue);
    }

    /// <inheritdoc />
    public virtual async Task<IList<ProductAttributeValue>> GetProductAttributeValuesAsync(int productAttributeMappingId)
    {
        var key = _staticCacheManager.PrepareKeyForDefaultCache(NopCatalogDefaults.ProductAttributeValuesByAttributeCacheKey, productAttributeMappingId);

        var query = from pav in _productAttributeValueRepository.Table
            orderby pav.DisplayOrder, pav.Id
            where pav.ProductAttributeMappingId == productAttributeMappingId
            select pav;
        var productAttributeValues = await _staticCacheManager.GetAsync(key, async () => await query.ToListAsync());

        return productAttributeValues;
    }

    /// <inheritdoc />
    public virtual async Task<ProductAttributeValue> GetProductAttributeValueByIdAsync(int productAttributeValueId)
    {
        return await _productAttributeValueRepository.GetByIdAsync(productAttributeValueId, cache => default);
    }

    /// <inheritdoc />
    public virtual async Task InsertProductAttributeValueAsync(ProductAttributeValue productAttributeValue)
    {
        await _productAttributeValueRepository.InsertAsync(productAttributeValue);
    }

    /// <inheritdoc />
    public virtual async Task UpdateProductAttributeValueAsync(ProductAttributeValue productAttributeValue)
    {
        await _productAttributeValueRepository.UpdateAsync(productAttributeValue);
    }

    #endregion

    #region Product attribute value pictures

    /// <inheritdoc />
    public virtual async Task DeleteProductAttributeValuePicturesAsync(IList<ProductAttributeValuePicture> valuePictures)
    {
        await _productAttributeValuePictureRepository.DeleteAsync(valuePictures);
    }

    /// <inheritdoc />
    public virtual async Task InsertProductAttributeValuePictureAsync(ProductAttributeValuePicture valuePicture)
    {
        await _productAttributeValuePictureRepository.InsertAsync(valuePicture);
    }

    /// <inheritdoc />
    public virtual async Task UpdateProductAttributeValuePictureAsync(ProductAttributeValuePicture valuePicture)
    {
        await _productAttributeValuePictureRepository.UpdateAsync(valuePicture);
    }

    /// <inheritdoc />
    public virtual async Task<IList<ProductAttributeValuePicture>> GetProductAttributeValuePicturesAsync(int valueId)
    {
        var allCacheKey = _staticCacheManager.PrepareKeyForDefaultCache(NopCatalogDefaults.ProductAttributeValuePicturesByValueCacheKey, valueId);

        var query = from pacp in _productAttributeValuePictureRepository.Table
            join p in _pictureRepository.Table on pacp.PictureId equals p.Id
            join pp in _productPictureRepository.Table on p.Id equals pp.PictureId
            where pacp.ProductAttributeValueId == valueId
            orderby pp.DisplayOrder, pacp.PictureId
            select pacp;

        var valuePictures = await _staticCacheManager.GetAsync(allCacheKey, async () => await query.ToListAsync())
                            ?? new List<ProductAttributeValuePicture>();

        return valuePictures;
    }

    /// <inheritdoc />
    public virtual ProductAttributeValuePicture FindProductAttributeValuePicture(IList<ProductAttributeValuePicture> source, int valueId, int pictureId)
    {
        return source.FirstOrDefault(vp => vp.ProductAttributeValueId == valueId && vp.PictureId == pictureId);
    }

    #endregion

    #region Predefined product attribute values

    /// <inheritdoc />
    public virtual async Task DeletePredefinedProductAttributeValueAsync(PredefinedProductAttributeValue ppav)
    {
        await _predefinedProductAttributeValueRepository.DeleteAsync(ppav);
    }

    /// <inheritdoc />
    public virtual async Task<IList<PredefinedProductAttributeValue>> GetPredefinedProductAttributeValuesAsync(int productAttributeId)
    {
        var key = _staticCacheManager.PrepareKeyForDefaultCache(NopCatalogDefaults.PredefinedProductAttributeValuesByAttributeCacheKey, productAttributeId);

        var query = from ppav in _predefinedProductAttributeValueRepository.Table
            orderby ppav.DisplayOrder, ppav.Id
            where ppav.ProductAttributeId == productAttributeId
            select ppav;

        var values = await _staticCacheManager.GetAsync(key, async () => await query.ToListAsync());

        return values;
    }

    /// <inheritdoc />
    public virtual async Task<PredefinedProductAttributeValue> GetPredefinedProductAttributeValueByIdAsync(int id)
    {
        return await _predefinedProductAttributeValueRepository.GetByIdAsync(id, cache => default);
    }

    /// <inheritdoc />
    public virtual async Task InsertPredefinedProductAttributeValueAsync(PredefinedProductAttributeValue ppav)
    {
        await _predefinedProductAttributeValueRepository.InsertAsync(ppav);
    }

    /// <inheritdoc />
    public virtual async Task UpdatePredefinedProductAttributeValueAsync(PredefinedProductAttributeValue ppav)
    {
        await _predefinedProductAttributeValueRepository.UpdateAsync(ppav);
    }

    #endregion

    #region Product attribute combinations

    /// <inheritdoc />
    public virtual async Task DeleteProductAttributeCombinationAsync(ProductAttributeCombination combination)
    {
        await _productAttributeCombinationRepository.DeleteAsync(combination);
    }

    /// <inheritdoc />
    public virtual async Task<IList<ProductAttributeCombination>> GetAllProductAttributeCombinationsAsync(int productId)
    {
        if (productId == 0)
            return new List<ProductAttributeCombination>();

        var combinations = await _productAttributeCombinationRepository.GetAllAsync(query =>
        {
            return from c in query
                orderby c.Id
                where c.ProductId == productId
                select c;
        }, cache => cache.PrepareKeyForDefaultCache(NopCatalogDefaults.ProductAttributeCombinationsByProductCacheKey, productId));

        return combinations;
    }

    /// <inheritdoc />
    public virtual async Task<ProductAttributeCombination> GetProductAttributeCombinationByIdAsync(int productAttributeCombinationId)
    {
        return await _productAttributeCombinationRepository.GetByIdAsync(productAttributeCombinationId, cache => default);
    }

    /// <inheritdoc />
    public virtual async Task<ProductAttributeCombination> GetProductAttributeCombinationBySkuAsync(string sku)
    {
        if (string.IsNullOrEmpty(sku))
            return null;

        sku = sku.Trim();

        var query = from pac in _productAttributeCombinationRepository.Table
            join p in _productRepository.Table on pac.ProductId equals p.Id
            orderby pac.Id
            where !p.Deleted && pac.Sku == sku
            select pac;
        var combination = await query.FirstOrDefaultAsync();

        return combination;
    }

    /// <inheritdoc />
    public virtual async Task InsertProductAttributeCombinationAsync(ProductAttributeCombination combination)
    {
        await _productAttributeCombinationRepository.InsertAsync(combination);
    }

    /// <inheritdoc />
    public virtual async Task UpdateProductAttributeCombinationAsync(ProductAttributeCombination combination)
    {
        await _productAttributeCombinationRepository.UpdateAsync(combination);
    }

    #endregion

    #region Product attribute combination pictures

    /// <inheritdoc />
    public virtual async Task DeleteProductAttributeCombinationPictureAsync(IList<ProductAttributeCombinationPicture> combinationPictures)
    {
        await _productAttributeCombinationPictureRepository.DeleteAsync(combinationPictures);
    }

    /// <inheritdoc />
    public virtual async Task InsertProductAttributeCombinationPictureAsync(ProductAttributeCombinationPicture combinationPicture)
    {
        await _productAttributeCombinationPictureRepository.InsertAsync(combinationPicture);
    }

    /// <inheritdoc />
    public virtual async Task UpdateProductAttributeCombinationPictureAsync(ProductAttributeCombinationPicture combinationPicture)
    {
        await _productAttributeCombinationPictureRepository.UpdateAsync(combinationPicture);
    }

    /// <inheritdoc />
    public virtual async Task<IList<ProductAttributeCombinationPicture>> GetProductAttributeCombinationPicturesAsync(int combinationId)
    {
        var allCacheKey = _staticCacheManager.PrepareKeyForDefaultCache(NopCatalogDefaults.ProductAttributeCombinationPicturesByCombinationCacheKey, combinationId);

        var query = from pacp in _productAttributeCombinationPictureRepository.Table
            join p in _pictureRepository.Table on pacp.PictureId equals p.Id
            join pp in _productPictureRepository.Table on p.Id equals pp.PictureId
            where pacp.ProductAttributeCombinationId == combinationId
            orderby pp.DisplayOrder, pacp.PictureId
            select pacp;

        var combinationPictures = await _staticCacheManager.GetAsync(allCacheKey, async () => await query.ToListAsync()) 
                                  ?? new List<ProductAttributeCombinationPicture>();

        return combinationPictures;
    }

    /// <inheritdoc />
    public virtual ProductAttributeCombinationPicture FindProductAttributeCombinationPicture(IList<ProductAttributeCombinationPicture> source, int combinationId, int pictureId)
    {
        return source.FirstOrDefault(pacp => pacp.ProductAttributeCombinationId == combinationId && pacp.PictureId == pictureId);
    }

    #endregion

    #endregion
}