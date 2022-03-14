use_assembly!("System.Console")
println = extern System.Console.WriteLine(System.Object)

function test_me(x)
must ->(x) x > 0 end
do
    println("hi there")
    println(x)
end

test_me(1) # runs
test_me(0) # fails