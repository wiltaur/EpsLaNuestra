namespace EpsLaNuestra.Domain.Interfaces.Repositories;

public interface IPatientMongoRepository
{
    Task SaveResourceAsync(string resourceType, string id, object fhirJson);
}