export class ProductModel {
    
    constructor(public id: number = 0,
    public title: string = "",
    public description: string = "",
    public price: number = 0,
    public thumbnail: string = "",
    public images: string[] = [],
    public rating: number = 0,
    public discountPercentage: number = 0,
    public brand: string = "",
    public sku: string = "",
    public availabilityStatus: string = "",
    public stock: number = 0,
    public weight: number = 0,
    public dimensions: { width: number; height: number; depth: number } = { width: 0, height: 0, depth: 0 },
    public warrantyInformation: string = "",
    public shippingInformation: string = "",
    public minimumOrderQuantity: number = 0,
    public tags: string[] = [],
    public reviews: Array<{ rating: number; comment: string; date: string; reviewerName: string; reviewerEmail?: string }> = []
    ) { }

}