using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.Dtos;
using API.Errors;
using AutoMapper;
using Core.Entities;
using Core.Interfaces;
using Core.Specifications;
using Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    
    public class ProductsController : BaseApiController
    {
        private readonly IGenericRepository<Product> _productsRepo;
        private readonly IGenericRepository<Product> _productBrandRepo;
        private readonly IGenericRepository<Product> _productTypeRepo;
        private readonly IMapper _mapper;
        private readonly StoreContext _storeContext;

        public ProductsController(IGenericRepository<Product> productsRepo,
        IGenericRepository<ProductBrand> productBrandRepo, 
        IGenericRepository<ProductType> productTypeRepo,
        IMapper mapper,
        StoreContext context
        ){
            _storeContext = context;
            _mapper = mapper;
            _productsRepo = productsRepo;
            _productBrandRepo = productsRepo;
            _productTypeRepo = productsRepo;
        }
        
        [HttpGet]
        public async Task<ActionResult<IReadOnlyList<ProductToReturnDto>>> GetProducts(){
            var spec = new ProductsWithTypesAndBrandsSpecification();

            var products = await _productsRepo.ListAsync(spec);

            return Ok(_mapper.Map<IReadOnlyList<Product>, IReadOnlyList<ProductToReturnDto>>
            (products));
        }

        [HttpGet("{id}")] //When we hit the endpoint
        public async Task<ActionResult<ProductToReturnDto>> GetProduct(int id){ //we pass the id

            //First thing to do is creating a new instance of specification with the id constructor
            var spec = new ProductsWithTypesAndBrandsSpecification(id);

            //Next step is getting the entity from productsRepo with the given specification
            var product =  await _productsRepo.GetEntityWithSpec(spec);

            return _mapper.Map<Product, ProductToReturnDto>(product);
        }

        [HttpGet("brands")]
        public async Task<ActionResult<IReadOnlyList<ProductBrand>>> GetProductBrands(){
            return Ok(await _productBrandRepo.ListAllAsync());
        }
        
        [HttpGet("types")]
        public async Task<ActionResult<IReadOnlyList<ProductBrand>>> GetProductTypes(){
            return Ok(await _productTypeRepo.ListAllAsync());
        }
        
        [HttpDelete("delete/{id}")] //When we hit the endpoint
        public async Task<ActionResult<ProductToReturnDto>> DeleteProduct(int id){ //we pass the id

            //First thing to do is creating a new instance of specification with the id constructor
            var spec = new ProductsWithTypesAndBrandsSpecification(id);

            //Next step is getting the entity from productsRepo with the given specification
            var product =  await _productsRepo.GetEntityWithSpec(spec);

            
            _storeContext.Products.Remove(product);
            _storeContext.SaveChanges();

            return _mapper.Map<Product, ProductToReturnDto>(product);
        }

        [HttpPost("add")]
        public async void AddProductPost(ProductToReturnDto newProduct){ 

            ProductType newProductType = new ProductType{
                Name = newProduct.ProductType
            };

            ProductBrand newProductBrand = new ProductBrand{
                Name = newProduct.ProductType
            };

            Product finalProduct = new Product{
                Name = newProduct.Name,
                Description = newProduct.Description,
                Price = newProduct.Price,
                PictureUrl = newProduct.PictureUrl,
                ProductType = newProductType,
                ProductTypeId = newProductType.Id,
                ProductBrand = newProductBrand,
                ProductBrandId = newProductBrand.Id
            };

            _storeContext.Products.Add(finalProduct);
            await _storeContext.SaveChangesAsync();
        }
        [HttpPut("update/{id}")]
        public void UpdateProductPut(int id, 
        ProductToReturnDto updatedProduct){

            var productToUpdate = _storeContext.Products.FirstOrDefault(p => p.Id == updatedProduct.Id);
            
            //Mapping from dto to entity
            ProductType updatedProductType = new ProductType{
                Name = updatedProduct.ProductType
            };

            ProductBrand updatedProductBrand = new ProductBrand{
                Name = updatedProduct.ProductBrand
            };

            //Mapping from dto to entity
            Product finalProduct = new Product{
                Name = updatedProduct.Name,
                Description = updatedProduct.Description,
                Price = updatedProduct.Price,
                PictureUrl = updatedProduct.PictureUrl,
                ProductType = updatedProductType,
                ProductTypeId = updatedProductType.Id,
                ProductBrand = updatedProductBrand,
                ProductBrandId = updatedProductBrand.Id
            };
            
            productToUpdate.Id = updatedProduct.Id;
            productToUpdate.Name = finalProduct.Name;
            productToUpdate.Description = finalProduct.Description;
            productToUpdate.Price = finalProduct.Price;
            productToUpdate.PictureUrl = finalProduct.PictureUrl;
            productToUpdate.ProductType = finalProduct.ProductType;
            productToUpdate.ProductTypeId = finalProduct.ProductTypeId;
            productToUpdate.ProductBrand = finalProduct.ProductBrand;
            productToUpdate.ProductBrandId = finalProduct.ProductBrandId;

            _storeContext.Products.Update(productToUpdate);
            _storeContext.SaveChanges();
        }
    }
}