package src.Util {
    public class FunctionUtil {

        public static function bind(fn: Function, thisArg: Object, ...rest): Function {
            var args: Array = rest.slice();

            return function (): * {
                return fn.apply(thisArg, args.concat(arguments.slice()));
            };
        }

    }
}