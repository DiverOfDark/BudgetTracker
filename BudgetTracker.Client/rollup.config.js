import svelte from 'rollup-plugin-svelte';
import resolve from 'rollup-plugin-node-resolve';
import commonjs from 'rollup-plugin-commonjs';
import livereload from 'rollup-plugin-livereload';
import { terser } from 'rollup-plugin-terser';
import { preprocess, createEnv, readConfigFile } from "svelte-ts-preprocess";
import typescript from "rollup-plugin-typescript2";

const production = !process.env.ROLLUP_WATCH;

const env = createEnv();
const compilerOptions = readConfigFile(env);
const opts = {
  env,
  compilerOptions: {
    ...compilerOptions,
    allowNonTsExtensions: true
  }
};

export default {
	input: 'src/main.ts',
	output: {
		sourcemap: true,
		format: 'iife',
		name: 'app',
		file: production ? 'out/bundle.js' : '../BudgetTracker/wwwroot/js/bundle.js'
	},
	plugins: [
		svelte({
			// enable run-time checks when not in production
			dev: !production,
			// we'll extract any component CSS out into
			// a separate file — better for performance
			css: css => {
				css.write(production ? 'out/bundle.css' : '../BudgetTracker/wwwroot/css/bundle.css');
			},
			preprocess: preprocess(opts)
		}),

		// If you have external dependencies installed from
		// npm, you'll most likely need these plugins. In
		// some cases you'll need additional configuration —
		// consult the documentation for details:
		// https://github.com/rollup/rollup-plugin-commonjs
		resolve(),
		commonjs(),
		typescript(),

		// Watch the `public` directory and refresh the
		// browser on changes when not in production
		!production && livereload('public'),

		// If we're building for production (npm run build
		// instead of npm run dev), minify
		production && terser()
	],
	watch: {
		clearScreen: false
	}
};
