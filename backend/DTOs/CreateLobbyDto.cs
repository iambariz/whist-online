namespace WhistOnline.API.DTOs;

public class CreateLobbyDto
{
    public required Guid Id { get; set; }
    //Todo: JWT Also or just switch to JWT, since we can read the ID
}