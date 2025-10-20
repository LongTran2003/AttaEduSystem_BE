namespace AttaEduSystem.Services.IServices
{
    public interface IAutoMapperService
    {
        TDestination Map<TSource, TDestination>(TSource source);
        void Map<TSource, TDestination>(TSource source, TDestination destination);
        IEnumerable<TDestination> MapCollection<TSource, TDestination>(IEnumerable<TSource> source);
    }
}
