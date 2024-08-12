using Microsoft.EntityFrameworkCore;
using AutoMapper;
using AutoMapper.QueryableExtensions;

public class ReproContext : DbContext
{
    public ReproContext(DbContextOptions<ReproContext> options) : base(options) { }

    public DbSet<Entity> Entities { get; set; }
    public DbSet<EntityType> EntityTypes { get; set; }
    public DbSet<Tag> Tags { get; set; }
}

public class Entity
{
    public int ID { get; set; }
    public EntityType Type { get; set; }
    public IEnumerable<Tag> Tags { get; set; }
}

public class EntityType
{
    public int ID { get; set; }
    public IEnumerable<Tag> Tags { get; set; }
}

public class Tag
{
    public int ID { get; set; }
}

public class DtoEntity
{
    public int ID { get; set; }
    public DtoEntityType Type { get; set; }
    public IEnumerable<DtoTag> Tags { get; set; }
}

public class DtoEntityType
{
    public int ID { get; set; }
    public IEnumerable<DtoTag> Tags { get; set; }
}

public class DtoTag
{
    public int ID { get; set; }
}

public class FinalResult
{
    public decimal Value { get; set; }
}

public class Profiles : Profile
{
    public Profiles()
    {
        CreateMap<Entity, DtoEntity>();
        CreateMap<EntityType, DtoEntityType>();
        CreateMap<Tag, DtoTag>();
    }
}

public class Program
{
    public static void Main(string[] args)
    {
        var builder = new DbContextOptionsBuilder<ReproContext>();

        builder.UseInMemoryDatabase("test");

        using (var context = new ReproContext(builder.Options))
        {
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();

            context.Entities.Add(new Entity
            {
                ID = 1,
                Tags = new List<Tag>()
                {
                    new Tag() { ID = 1 }
                },
                Type = new EntityType
                {
                    ID = 1,
                    Tags = new List<Tag>()
                    {
                        new Tag() { ID = 2 }
                    }
                }
            });
            context.SaveChanges();
        }

        using (var context = new ReproContext(builder.Options))
        {

            var cfg = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<Profiles>();
            });

            //Works
            var test = context.Entities
                .ProjectTo<DtoEntity>(cfg)
                .Select(f => new FinalResult
                {
                    Value = f.Tags.Count()
                })
                .Single();
            Console.WriteLine(test.Value);

            //Works
            var test3 = context.Entities
                .Select(f => new DtoEntity()
                {
                    ID = f.ID,
                    Tags = f.Tags.Select(t => new DtoTag() { ID = t.ID }),
                    Type = new DtoEntityType()
                    {
                        ID = f.Type.ID,
                        Tags = f.Type.Tags.Select(t => new DtoTag() { ID = t.ID })
                    }
                })
                .Select(f => new FinalResult
                {
                    Value = f.Type.Tags.Count()
                })
                .Single();
            Console.WriteLine(test3.Value);

            //Exception
            var test2 = context.Entities
                .ProjectTo<DtoEntity>(cfg)
                .Select(f => new FinalResult
                {
                    Value = f.Type.Tags.Count()
                })
                .Single();

        }
    }

}