export class ProductModel{
    constructor(public title: string = "",public price: number = 0, public description: string = "", public imageUrl: string = ""){
        this.title = title;
        this.price = price;
        this.description = description;
        this.imageUrl = imageUrl;
    }
}