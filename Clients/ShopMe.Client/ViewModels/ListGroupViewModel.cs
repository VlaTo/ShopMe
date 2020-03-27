using System;
using System.Collections;
using System.Collections.Generic;
using Prism.Mvvm;
using ShopMe.Application.Observable.Collections;

namespace ShopMe.Client.ViewModels
{
    public class ListGroupViewModel : List<ListDescriptionViewModel>
    {

        public string Title { get; set; }
    }
}