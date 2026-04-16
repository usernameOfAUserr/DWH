import {createRouter, createWebHistory, type RouteRecordRaw} from "vue-router"
import AnalysisPage from "@/Components/Analysis/AnalysisPage.vue";
import ImportPage from "@/Components/Import/ImportPage.vue";

const routes: RouteRecordRaw[] = [
    {
        path: "/",
        component: AnalysisPage,
    },
    {
        path: "/import",
        component: ImportPage,
    }
]

export const router = createRouter({
    history: createWebHistory(),
    routes,
});
