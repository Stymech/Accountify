function basketArticleInfo() {
    var basketIDs = [];
    basket.forEach(function (item) {
        basketIDs.push(item.id + '-' + item.quantity);
    });
    return basketIDs;
}
function refreshBasket() {
    basket = basket.filter(item => item.quantity > 0);
    if (basket.length > 0) {
        $('#basket').show();
    }
    else {
        $('#basket').hide();
    }
    var basketList = $('#basketList');
    basketList.empty();
    totalSum = 0;

    basket.forEach(function (item) {
        // Create a unique ID for the quantity input field based on the article name
        var quantityId = 'quantity' + item.id;
        var deleteID = 'delete' + item.id;
        var deleteArticle = '<a href="#"><i class="fas fa-trash-alt" id="' + deleteID + '" style="color: red;" onclick="deleteArticle(event)"></i></a>';

        basketList.append('<tr>');
        basketList.append('<td>' + deleteArticle + '</td>');
        basketList.append('<td>' + item.id + '</td>');
        basketList.append('<td>' + item.name + '</td>');
        basketList.append('<td>' + item.price + '€' + '</td>');
        basketList.append('<td><input type="number" id="' + quantityId + '" value="' + item.quantity + '" onchange="updateQuantity(\'' + quantityId + '\')"></td>');
        basketList.append('<td id="subtotal' + item.id + '">' + item.subtotal + '€' + '</td>');
        basketList.append('</tr>');

        totalSum += parseFloat(item.subtotal);
    });
    totalSum = (Math.floor(totalSum * 100) / 100).toFixed(2)
    basketList.append('<tr>');
    basketList.append('<td colspan="4"></td>');
    basketList.append('<td>Total:</td>');
    basketList.append('<td>' + totalSum + '€' + '</td>');
    basketList.append('</tr>');

    $('#basketData').val(basketArticleInfo());
    $('#basketSum').val(totalSum);
}
function getSubtotal(price, quantity) {
    return (Math.floor(price * quantity * 100) / 100).toFixed(2);
}

function deleteArticle(event) {
    deleteID = event.currentTarget.id.replace('delete', '');
    var item = basket.find(item => item.id == deleteID);
    item.quantity = 0;
    refreshBasket();
}

function updateQuantity(quantityID) {
    // Find the corresponding item based on the inputId
    var item = basket.find(item => 'quantity' + item.id === quantityID);

    if (item) {
        var quantity = parseInt($('#' + quantityID).val());
        item.quantity = quantity;
        item.subtotal = getSubtotal(item.price, quantity);
        $('#subtotal' + item.id).text(item.subtotal);
    }
    refreshBasket();
}

function closeDropdown() {
    isDropdownOpen = false;
    $('#articleDropdown').empty(); // Clear existing items
}

function showDropdown(inputText) {
    function addToDropdown(articles) {
        var itemsList = $('#articleDropdown'); // Parent element containing the items

        articles.forEach(function (article) {
            var listItem = $('<li>' + article.ArticleID + ' - ' + article.ArticleName + ' - ' + article.ArticlePrice + '€' + '</li>');

            // Attach a click event handler to the newly created list item
            listItem.on('click', function () {
                // Get the data attributes or text of the clicked item
                var articleID = article.ArticleID;
                var articleName = article.ArticleName;
                var articlePrice = article.ArticlePrice;

                // Fill the input field with the selected article
                $('#articleInput').val(articleID + ' - ' + articleName + ' - ' + articlePrice + '€');
                itemsList.hide();
            });

            itemsList.append(listItem);
        });
    }
    closeDropdown()
    var dropdownArticles = [];
    listArticles.forEach(function (article) {
        var articleName = article.ArticleName.toLowerCase();
        var articleID = article.ArticleID.toString();

        if (articleName.includes(inputText) || articleID.includes(inputText)) {
            isDropdownOpen = true;
            dropdownArticles.push(article)
        }
    });

    // Show the dropdown below the input field
    if (isDropdownOpen) {
        addToDropdown(dropdownArticles);
        var inputPosition = $('#articleInput').offset();
        var inputWidth = $('#articleInput').outerWidth();
        $('#articleDropdown').css({
            'position': 'absolute',
            'left': inputPosition.left,
            'top': inputPosition.top + $('#articleInput').outerHeight(),
            'width': inputWidth,
            'display': 'block'
        });
    }
}


//////////////////////////////////////////////////////////////////////////////

var basket = [];
var totalSum;

$('#basket').hide();
$('#basketData').hide();
$('#basketSum').hide();

$('#newReceipt').on('submit', function () {
    $('#basketData').val(basketArticleInfo());
    $('#basketSum').val(totalSum);
});

$(document).ready(function () {
    var isDropdownOpen = false;
    $('#articleInput').on('input', function () {
        var inputText = $(this).val().toLowerCase();
       
        showDropdown(inputText);
    });

    $('#btnAddArticle').on('click', function () {
        var selectedOption = $('#articleInput').val();
        var selectedOptionText = selectedOption.split(' - ');
        var selectedID = selectedOptionText[0];
        var selectedName = selectedOptionText[1];
        var selectedQuantity = parseInt($('#articleQuantity').val());

        if (selectedID) {
            // Check if the article exists in database
            var articleInDB = listArticles.find(item => item.ArticleID === parseInt(selectedID));

            if (articleInDB) {
                // Check if the article is already in the basket
                var basketArticle = basket.find(item => item.id === parseInt(selectedID));

                if (basketArticle) {
                    // If the article is already in the basket, increase its quantity
                    basketArticle.quantity += selectedQuantity;
                    basketArticle.subtotal = getSubtotal(basketArticle.price, basketArticle.quantity);
                }
                else {
                    // Otherwise, add a new item to the basket with new qunatity
                    basket.push({
                        id: articleInDB.ArticleID,
                        name: articleInDB.ArticleName,
                        price: articleInDB.ArticlePrice,
                        quantity: selectedQuantity,
                        subtotal: getSubtotal(articleInDB.ArticlePrice, selectedQuantity),
                    });
                }
            }
            refreshBasket();
        }
    });
    refreshBasket();
});

if (window.location.href.includes('/Edit')) {
    function populateBasket() {
        selectedReceipt.ReceiptArticles.forEach(function (receiptArticle) {
            const quantity = quantities[receiptArticle.ArticleID];
            basket.push({
                id: receiptArticle.ArticleID,
                name: receiptArticle.ArticleName,
                price: receiptArticle.ArticlePrice,
                quantity: quantity,
                subtotal: getSubtotal(receiptArticle.ArticlePrice, quantity),
            });
        });
    }
    function populateBuyerInfo() {
        document.querySelector('input[name="BuyerName"]').value = buyerInfo.BuyerName;
        document.querySelector('input[name="BuyerAddress"]').value = buyerInfo.BuyerAddress;
        document.querySelector('input[name="BuyerTel"]').value = buyerInfo.BuyerTel;
        document.querySelector('input[name="BuyerEmail"]').value = buyerInfo.BuyerEmail;
        var dateParts = selectedReceipt.ReceiptDate.split('/');
        var formattedDate = dateParts[2] + '-' + dateParts[1] + '-' + dateParts[0];
        document.querySelector('input[name="ReceiptDate"]').value = formattedDate;

    }
    populateBasket();
    populateBuyerInfo();
}