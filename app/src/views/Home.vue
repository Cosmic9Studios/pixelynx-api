<template>
  <div class="home">
    <img alt="Vue logo" src="../assets/logo.png">
    <!--<HelloWorld msg="Welcome to Your Vue.js + TypeScript App"/>-->
    <AssetBrowser />
  </div>
</template>

<script lang="ts">
import { Component, Vue } from 'vue-property-decorator';
import HelloWorld from '@/components/HelloWorld.vue'; // @ is an alias to /src
import AssetBrowser from '@/components/AssetBrowser.vue'; // @ is an alias to /src

@Component({
  components: {
    HelloWorld,
    AssetBrowser
  }
})
export default class Home extends Vue {}
</script>
