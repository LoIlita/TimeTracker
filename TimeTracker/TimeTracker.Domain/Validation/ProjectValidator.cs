using System;
using TimeTracker.Domain.Entities;

namespace TimeTracker.Domain.Validation;

public static class ProjectValidator
{
    public static void ValidateProject(Project project)
    {
        if (project == null)
            throw new ArgumentNullException(nameof(project));

        if (string.IsNullOrWhiteSpace(project.Name))
            throw new ValidationException("Nazwa projektu nie może być pusta.");

        if (project.Name.Length > 100)
            throw new ValidationException("Nazwa projektu nie może być dłuższa niż 100 znaków.");

        if (project.Description?.Length > 500)
            throw new ValidationException("Opis projektu nie może być dłuższy niż 500 znaków.");

        if (project.CreatedAt > DateTime.Now)
            throw new ValidationException("Data utworzenia projektu nie może być z przyszłości.");

        if (project.LastModifiedAt.HasValue && project.LastModifiedAt.Value < project.CreatedAt)
            throw new ValidationException("Data modyfikacji nie może być wcześniejsza niż data utworzenia.");

        if (!Enum.IsDefined(typeof(ProjectType), project.Type))
            throw new ValidationException("Nieprawidłowy typ projektu.");
    }
} 