var app = angular.module('orderApp.orderForm', []);

app.controller('orderFormController', function($scope, $http) {
    $scope.formHeader = 'Items available to purchase';
    $scope.orderHistoryHeader = 'Successful Orders';
    $scope.SubmittedOrders = [];
    $scope.flash = null;

    if (availableItems != null) {
        $scope.availableItems = availableItems;
        $scope.itemDictionary = {};
        angular.forEach(availableItems, function(value, key) {
            $scope.itemDictionary[value.item_id] = value;
        });

    }

    $scope.updatePreSubmitStats = function(itemId) {
        var item = $scope.itemDictionary[itemId];
        item.subtotal = !isNaN(item.quantity) && item.quantity != 0 ? item.price * item.quantity : null;
        var totalQuantity = 0;
        var totalPrice = 0;
        angular.forEach(availableItems, function (value, key) {
            totalQuantity += !isNaN(item.quantity) && item.quantity != 0 ? Number(value.quantity) : 0;
            totalPrice += !isNaN(value.subtotal) ? value.subtotal : 0;
        });
        $scope.TotalCount = totalQuantity;
        $scope.OrderSubTotal = totalPrice;
    }

    $scope.submitOrder = function() {
        $http.post('/Order/SubmitOrder', $scope.availableItems).
        success(function (data, status, headers, config) {
            if (data["SuccessfulTransaction"]) {
                var order = data["Order"];
                updateOrderStats(order);
                $scope.SubmittedOrders.push(data["Order"]);
                zeroOrder();
                $scope.TotalCount = 0;
                $scope.OrderSubTotal = 0;
            } else {
                $scope.flash = "There was a problem submitting your order.";
            }
        });
    }

    var updateOrderStats = function(order)
    {
        order.quantity = 0;
        order.total = 0;
        angular.forEach(order.items, function(value, key) {
            order.quantity += value.quantity;
            order.total += value.price * value.quantity;
            value.description = $scope.itemDictionary[value.item_id].description;
        });
    }

    $scope.calculatePrice = function (items) {
        var quantity = 0;
        angular.forEach(items, function (value, key) {
            quantity += value.quantity;
        });
        return quantity;
    }
});