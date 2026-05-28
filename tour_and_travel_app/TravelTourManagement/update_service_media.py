file_path = '/Users/iniyan/Intern/Presidio-intern/tour_and_travel_app/TravelTourManagement/TravelTourManagement.Business/Services/PackageService.cs'
with open(file_path, 'r') as f:
    content = f.read()

import re

old_method_sig = "public async Task<Guid> CreatePackageAsync(Guid userId, CreatePackageRequest request, CancellationToken cancellationToken = default)"
new_method_sig = "public async Task<Guid> CreatePackageAsync(Guid userId, CreatePackageRequest request, List<IFormFile>? mediaFiles = null, CancellationToken cancellationToken = default)"
content = content.replace(old_method_sig, new_method_sig)

old_media_mapping = '''            PackageMedia = request.Media?.Select(m => new PackageMedium
            {
                FilePath = m.FilePath,
                FileName = m.FileName,
                Caption = m.Caption,
                DisplayOrder = m.DisplayOrder,
                IsPrimary = m.IsPrimary,
                Category = Enum.Parse<TravelTourManagement.DataAccess.Enums.MediaCategory>(m.Category, true),
                UploadedAt = DateTime.UtcNow
            }).ToList() ?? new(),'''

new_media_mapping = '''            PackageMedia = new List<PackageMedium>(),'''
content = content.replace(old_media_mapping, new_media_mapping)

old_repo_call = '''        await _packageRepository.CreatePackageWithDetailsAsync(package, cancellationToken);

        return package.Id;'''

new_repo_call = '''        // Process actual media files
        if (mediaFiles != null && mediaFiles.Any() && request.Media != null)
        {
            var uploadDirectory = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..", "TravelTourManagement.DataAccess", "Uploads", "Packages");
            if (!System.IO.Directory.Exists(uploadDirectory))
            {
                System.IO.Directory.CreateDirectory(uploadDirectory);
            }

            foreach (var file in mediaFiles)
            {
                // Find matching metadata in request.Media based on FileName
                var meta = request.Media.FirstOrDefault(m => m.FileName == file.FileName);
                if (meta != null)
                {
                    var uniqueFileName = $"{Guid.NewGuid()}_{file.FileName}";
                    var filePath = System.IO.Path.Combine(uploadDirectory, uniqueFileName);

                    using (var stream = new System.IO.FileStream(filePath, System.IO.FileMode.Create))
                    {
                        await file.CopyToAsync(stream, cancellationToken);
                    }

                    package.PackageMedia.Add(new PackageMedium
                    {
                        FilePath = $"/api/packages/media/{uniqueFileName}",
                        FileName = uniqueFileName,
                        Caption = meta.Caption,
                        DisplayOrder = meta.DisplayOrder,
                        IsPrimary = meta.IsPrimary,
                        Category = Enum.Parse<TravelTourManagement.DataAccess.Enums.MediaCategory>(meta.Category, true),
                        MimeType = file.ContentType,
                        FileSizeBytes = file.Length,
                        UploadedAt = DateTime.UtcNow
                    });
                }
            }
        }

        await _packageRepository.CreatePackageWithDetailsAsync(package, cancellationToken);

        return package.Id;'''
content = content.replace(old_repo_call, new_repo_call)

with open(file_path, 'w') as f:
    f.write(content)
