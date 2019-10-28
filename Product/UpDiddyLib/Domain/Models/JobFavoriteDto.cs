using System;
public class JobFavoriteDto
{
    public Guid JobPostingGuid { get; set; }
    public DateTime PostingDateUTC { get; set; }
    public DateTime ExpirationDateUTC { get; set; }
    public bool HasApplied { get; set; }
    public string CompanyLogoUrl { get; set; }
    public string CompanyName { get; set; }
    public string Title { get; set; }
    public string City { get; set; }
    public string Province { get; set; }
    public string SemanticJobPath { get; set; }

}


