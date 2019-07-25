import svelte from 'rollup-plugin-svelte';
import resolve from 'rollup-plugin-node-resolve';
import commonjs from 'rollup-plugin-commonjs';
import livereload from 'rollup-plugin-livereload';
import { terser } from 'rollup-plugin-terser';
import { preprocess, createEnv, readConfigFile } from "@pyoner/svelte-ts-preprocess";
import typescript from "rollup-plugin-typescript2";
import progress from 'rollup-plugin-progress';
import babel from 'rollup-plugin-babel';
import { sizeSnapshot } from "rollup-plugin-size-snapshot";

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

const onwarn = warning => {
	if (warning.code === 'CIRCULAR_DEPENDENCY') {
	  return
	}
	
	var cwd = process.cwd() + "/";

	var fileName = warning.loc.file.replace(cwd, "");

	console.warn(`(!) ${warning.message} (${fileName}:${warning.loc.line}:${warning.loc.column}):\n${warning.frame}\n\n`)
  }

export default {
	input: 'src/main.js',
	output: {
		sourcemap: true,
		format: 'iife',
		name: 'app',
		file: '../BudgetTracker/wwwroot/js/bundle.js'
	},
	onwarn,
	plugins: [
		progress(),
		sizeSnapshot(), 
		svelte({
			// enable run-time checks when not in production
			dev: !production,
			// we'll extract any component CSS out into
			// a separate file — better for performance
			css: css => {
				css.write('../BudgetTracker/wwwroot/css/bundle.css');
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
		babel({
			exclude: 'node_modules/**' // only transpile our source code
		  }),

		// If we're building for production (npm run build
		// instead of npm run dev), minify
		production && terser()
	],
	watch: {
		clearScreen: false
	}
};
