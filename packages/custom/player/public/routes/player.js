(function () {
  'use strict';

  angular
    .module('mean.player')
    .config(player);

  player.$inject = ['$stateProvider'];

  function player($stateProvider) {
    $stateProvider.state('player example page', {
      url: '/player/example',
      templateUrl: 'player/views/index.html'
    });
  }

})();
