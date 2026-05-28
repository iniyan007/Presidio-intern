file_path = '/Users/iniyan/Intern/Presidio-intern/tour_and_travel_app/TravelTourManagement/TravelTourManagement.Business/Services/BookingService.cs'
with open(file_path, 'r') as f:
    content = f.read()

import re

if "using Microsoft.AspNetCore.Http;" not in content:
    content = content.replace("using TravelTourManagement.DataAccess.DTOs.Bookings;", "using TravelTourManagement.DataAccess.DTOs.Bookings;\nusing Microsoft.AspNetCore.Http;")

old_method_sig = "public async Task<BookingResponse> CreateBookingAsync(Guid userId, CreateBookingRequest request, CancellationToken cancellationToken = default)"
new_method_sig = "public async Task<BookingResponse> CreateBookingAsync(Guid userId, CreateBookingRequest request, List<IFormFile>? documentFiles = null, CancellationToken cancellationToken = default)"
content = content.replace(old_method_sig, new_method_sig)

old_file_logic = '''                if (t.AadharCardFile != null && t.AadharCardFile.Length > 0)
                {
                    string uniqueFileName = Guid.NewGuid().ToString() + "_" + t.AadharCardFile.FileName;
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        t.AadharCardFile.CopyTo(fileStream); 
                    }
                    traveler.TravelDocuments.Add(new TravelDocument
                    {
                        DocumentType = "Aadhar Card",
                        FileName = uniqueFileName,
                        OriginalFilename = t.AadharCardFile.FileName,
                        FilePath = $"/uploads/travel_documents/{uniqueFileName}",
                        FileSizeBytes = t.AadharCardFile.Length,
                        MimeType = t.AadharCardFile.ContentType,
                        UploadedAt = DateTime.UtcNow
                    });
                }

                if (t.PassportFile != null && t.PassportFile.Length > 0)
                {
                    string uniqueFileName = Guid.NewGuid().ToString() + "_" + t.PassportFile.FileName;
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        t.PassportFile.CopyTo(fileStream);
                    }
                    traveler.TravelDocuments.Add(new TravelDocument
                    {
                        DocumentType = "Passport",
                        FileName = uniqueFileName,
                        OriginalFilename = t.PassportFile.FileName,
                        FilePath = $"/uploads/travel_documents/{uniqueFileName}",
                        FileSizeBytes = t.PassportFile.Length,
                        MimeType = t.PassportFile.ContentType,
                        UploadedAt = DateTime.UtcNow
                    });
                }'''

new_file_logic = '''                if (!string.IsNullOrEmpty(t.AadharCardFileName) && documentFiles != null)
                {
                    var file = documentFiles.FirstOrDefault(f => f.FileName == t.AadharCardFileName);
                    if (file != null && file.Length > 0)
                    {
                        string uniqueFileName = Guid.NewGuid().ToString() + "_" + file.FileName;
                        string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            file.CopyTo(fileStream); 
                        }
                        traveler.TravelDocuments.Add(new TravelDocument
                        {
                            DocumentType = "Aadhar Card",
                            FileName = uniqueFileName,
                            OriginalFilename = file.FileName,
                            FilePath = $"/uploads/travel_documents/{uniqueFileName}",
                            FileSizeBytes = file.Length,
                            MimeType = file.ContentType,
                            UploadedAt = DateTime.UtcNow
                        });
                    }
                }

                if (!string.IsNullOrEmpty(t.PassportFileName) && documentFiles != null)
                {
                    var file = documentFiles.FirstOrDefault(f => f.FileName == t.PassportFileName);
                    if (file != null && file.Length > 0)
                    {
                        string uniqueFileName = Guid.NewGuid().ToString() + "_" + file.FileName;
                        string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            file.CopyTo(fileStream);
                        }
                        traveler.TravelDocuments.Add(new TravelDocument
                        {
                            DocumentType = "Passport",
                            FileName = uniqueFileName,
                            OriginalFilename = file.FileName,
                            FilePath = $"/uploads/travel_documents/{uniqueFileName}",
                            FileSizeBytes = file.Length,
                            MimeType = file.ContentType,
                            UploadedAt = DateTime.UtcNow
                        });
                    }
                }'''

content = content.replace(old_file_logic, new_file_logic)

with open(file_path, 'w') as f:
    f.write(content)
