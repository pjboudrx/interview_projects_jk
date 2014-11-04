var app = angular.module('orderApp.orderForm', []);

app.controller('orderFormController', function($scope, $http, $location) {
    $scope.formHeader = 'Items available to purchase';
    if (availableItems != null) {
        $scope.availableItems = availableItems;
        $scope.itemDictionary = {};
        angular.forEach(availableItems, function(value, key) {
            $scope.itemDictionary[value.item_id] = value;
        });
        console.log($scope.itemDictionary);

    }

    $scope.updateRunningTotal = function(itemId) {
        var item = $scope.itemDictionary[itemId];
        item.subtotal = !isNaN(item.quantity) && item.quantity != 0 ? item.price * item.quantity : null;
        var totalQuantity = 0;
        var totalPrice = 0;
        angular.forEach(availableItems, function (value, key) {
            totalQuantity += !isNaN(item.quantity) && item.quantity != 0 ? Number(value.quantity) : 0;
            totalPrice += !isNaN(value.subtotal) ? value.subtotal : 0;
        });
        console.log(totalPrice, totalQuantity);
        $scope.TotalCount = totalQuantity;
        $scope.OrderSubTotal = totalPrice;
    }

    $scope.submitOrder = function() {
        $http.post('/Order/SubmitOrder', $scope.availableItems).
        success(function(data, status, headers, config) {
	        
        });
    }
});