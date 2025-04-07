namespace Nop.Data.Mapping;

public partial class NopEntityDescriptor
{
    public NopEntityDescriptor()
    {
        Fields = [];
    }

    public string EntityName { get; set; }
    public string SchemaName { get; set; }
    public ICollection<NopEntityFieldDescriptor> Fields { get; set; }
}