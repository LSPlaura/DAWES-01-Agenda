namespace AgendaContactos.Back.Models.Enums;

public enum Response
{
    Ok = 200,
    Created = 201,
    Accepted = 202,
    
    BadRequest = 400,
    NotFound = 404,
    Conflict = 409,
    UnprocessableEntity = 422,
}