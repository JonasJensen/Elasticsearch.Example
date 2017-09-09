angular.module('elasticsearch', ['ui.bootstrap'])
    .filter('to_trusted', ['$sce', function ($sce) {
        return function (text) {
            return $sce.trustAsHtml(text);
        };
    }])
    .controller('searchController', function ($scope, $http) {
        var activeFilters = ['potato', 'tomato', 'chillies', 'green-pepper'];


        var search = function () {
            $http.get("/api/search?q=" + $scope.query).success(function (data) {
                $scope.isLoading = false;
                $scope.results = data.Results;
                $scope.aggs = data.AggregationsByTags;
                $scope.total = 0;
                if (data.Results.length === 0)
                    $scope.message = "no results";
                else {
                    $scope.total = data.Total;
                    $scope.took = data.ElapsedMilliseconds;
                }
            });
        }

        $scope.isActive = function (tag) {
            return activeFilters.length > 0 && activeFilters.indexOf(tag) >= 0;
        }

        var searchByCategory = function () {
            $http.post("/api/searchbycategory", { "q": $scope.query, "categories": activeFilters }).success(function (data) {
                $scope.isLoading = false;
                $scope.results = data.Results;
                $scope.aggs = data.AggregationsByTags;
                $scope.total = 0;
                if (data.Results.length === 0)
                    $scope.message = "no results";
                else {
                    $scope.total = data.Total;
                    $scope.took = data.ElapsedMilliseconds;
                }
            });
        }

        $scope.toggleFilters = function (tag) {
            $scope.isLoading = true;
            var index = activeFilters.indexOf(tag);
            if (index === -1) {
                activeFilters.push(tag);
            } else {
                activeFilters.splice(index, 1);
            }

            if (activeFilters.length > 0) {
                searchByCategory();
            } else {
                search();
            }
        }

        $scope.setQuery = function (q) {
            $scope.query = q;
            $scope.suggested = {};
            search();
        }

        $scope.search = function () {
            $scope.suggested = {};
            $scope.currentItem = null;
            $scope.message = "";
            $scope.isLoading = true;
            activeFilters = [];
            search();
        }

        $scope.autocomplete = function (q) {
            var words = q.split(" ");
            var currentWord = words[words.length - 1];

            return $http.get("/api/autocomplete?q=" + currentWord).then(function (response) {
                return response.data;
            });
        };

        var typeaheadLastValue = "";
        $scope.$watch('query', function (newVal, oldVal) {
            typeaheadLastValue = oldVal || "";
        });

        $scope.typeaheadOnSelect = function (item, model, label, event) {
            var words = typeaheadLastValue.split(" ");
            words[words.length - 1] = item;
            $scope.query = words.join(" ");
           
            //$scope.query = query;
        }

        $scope.open = function (size) {

            var modalInstance = $uibModal.open({
                animation: $scope.animationsEnabled,
                templateUrl: 'myModalContent.html',
                controller: 'ModalInstanceCtrl',
                size: size,
                resolve: {
                    items: function () {
                        return $scope.results;
                    }
                }
            });

            modalInstance.result.then(function (selectedItem) {
                $scope.selected = selectedItem;
            }, function () {
                $log.info('Modal dismissed at: ' + new Date());
            });
        };



    });