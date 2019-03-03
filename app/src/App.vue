<template>
  <div id="app">
    <router-view />
  </div>
</template>

<script>
import HelloWorld from './components/HelloWorld';

export default {
  name: 'App',
  components: {
    HelloWorld
  },
  data() {
    return {
      //
    };
  }
};
</script>
