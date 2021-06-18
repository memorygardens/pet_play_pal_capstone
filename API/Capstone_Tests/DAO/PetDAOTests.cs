using Capstone.DAO;
using Capstone.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;

namespace Capstone_Tests.DAO
{
    [TestClass]
    public class PetDAOTests : DAOTests
    {
        PetDAO dao;

        [TestInitialize]
        public override void Setup()
        {
            base.Setup();
            dao = new PetDAO(connectionString);
        }
        [DataTestMethod]
        [DataRow(2, "Maggie")]
        public void GetPetByID_Test(int petId, string expectedPetName)
        {
            //arrange

            //act
            Pet actualPet = dao.GetPetById(petId);

            //assert
            Assert.AreEqual(expectedPetName, actualPet.PetName);
        }

        [TestMethod]
        public void AddPet_test_series()
        {
            //"data rows"
            List<Pet> PetsToTest = new List<Pet>()
            {
                new Pet(){
                    UserId=5,
                    PetName="Jada",
                    Birthday=new DateTime(2005,8,15),
                    Sex = 'F',
                    PetTypeId = dao.GetPetTypeId("cat"),
                    Breed = "Maine Coon",
                    Color = "Brown",
                    Bio = "A sweet snuggly old girl",
                    PersonalityIds = new int[]{1,3}
                }
            };


            foreach (Pet pet in PetsToTest)
            {
                int newPetId = dao.AddPet(pet);
                Pet actuallyAddedPet = dao.GetPetById(newPetId);
                Assert.IsTrue(Pet.AreEquivalent(pet, actuallyAddedPet),"The actually added pet and the original pet do not appear to be equivalent");
            }
        }




    }
}
